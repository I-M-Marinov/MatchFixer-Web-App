using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Bets;
using MatchFixer.Core.DTOs.Limits;
using MatchFixer.Core.Helpers;
using MatchFixer.Core.Services;
using MatchFixer.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace MatchFixer_Web_App.Controllers
{
	[Authorize]
	[ApiController]
	[Route("[controller]/[action]")]
	public class BetSlipController : ControllerBase
	{
		private readonly ISessionService _sessionService;
		private readonly MatchFixerDbContext _dbContext;
		private readonly IOddsBoostService _oddsBoostService;
		private readonly IBettingLimitsService _limits;

		public BetSlipController(ISessionService sessionService, MatchFixerDbContext dbContext,
			IOddsBoostService oddsBoostService, IBettingLimitsService limits)
		{
			_sessionService = sessionService;
			_dbContext = dbContext;
			_oddsBoostService = oddsBoostService;
			_limits = limits;
		}

		public class SlipLimitsRequest
		{
			public List<SlipLimitsLeg> Legs { get; set; } = new();
			public decimal TotalOdds { get; set; }
		}

		public class SlipLimitsLeg
		{
			public Guid MatchId { get; set; }
			public string? Market { get; set; }
		}

		/// <summary>Max stake / max win for the slip being built (shown in the bet slip; enforced again on placing).</summary>
		[HttpPost]
		public async Task<IActionResult> Limits([FromBody] SlipLimitsRequest request, CancellationToken ct)
		{
			var legs = (request?.Legs ?? new())
				.Where(l => l.MatchId != Guid.Empty)
				.Select(l => new SlipLeg(l.MatchId,
					Enum.TryParse<BettingMarket>(l.Market, true, out var m) ? m : BettingMarket.MatchResult))
				.ToList();

			var limits = await _limits.GetSlipLimitsAsync(legs, request?.TotalOdds ?? 0, ct);

			return new JsonResult(new
			{
				legs = limits.Legs,
				hasExtraMarkets = limits.HasExtraMarkets,
				maxStake = limits.MaxStake,
				maxWin = limits.MaxWin,
				maxStakeForWin = limits.MaxStakeForWin,
				promoMaxStake = limits.PromoMaxStake,
				effectiveMaxStake = limits.EffectiveMaxStake
			});
		}

		[HttpGet]
		public async Task<IActionResult> Get([FromServices] MatchFixerDbContext dbContext, [FromServices] IOddsBoostService oddsBoostService)
		{
			var betSlip = _sessionService.GetBetSlipState();

			if (betSlip == null || !betSlip.Bets.Any())
				return new JsonResult(new
				{
					userId = (string?)null,
					bets = new object[] { },
					totalOdds = 0.0,
					stakeAmount = 0.0
				});

			var matchIds = betSlip.Bets.Select(b => b.MatchId).ToList();
			var matches = await dbContext.MatchEvents
				.Where(m => matchIds.Contains(m.Id))
				.ToDictionaryAsync(m => m.Id);

			var marketOdds = await dbContext.MatchOdds
				.AsNoTracking()
				.Where(o => matchIds.Contains(o.MatchEventId))
				.ToListAsync();

			// Update session bets with latest effective odds (boosts respected)
			foreach (var bet in betSlip.Bets)
			{
				if (!matches.TryGetValue(bet.MatchId, out var match))
					continue;

				bet.IsPostponed = match.IsPostponed;

				// Extra markets: re-price from MatchOdds (no boosts on these markets)
				if (bet.Market != BettingMarket.MatchResult)
				{
					var latest = marketOdds.FirstOrDefault(o => o.MatchEventId == bet.MatchId
															 && o.Market == bet.Market
															 && o.Selection == bet.Selection);
					if (latest != null)
					{
						bet.Odds = latest.Odds;
						bet.Line = latest.Line;
					}
					continue;
				}

				var (home, draw, away, _) = await oddsBoostService.GetEffectiveOddsAsync(
					match.Id,
					match.HomeOdds,
					match.DrawOdds,
					match.AwayOdds,
					ct: default
				);

				// Only update odds if effective odds exist, otherwise keep current session odds
				if (Enum.TryParse<MatchPick>(bet.SelectedOption, ignoreCase: true, out var pick))
				{
					bet.Odds = pick switch
					{
						MatchPick.Home => home ?? bet.Odds,
						MatchPick.Draw => draw ?? bet.Odds,
						MatchPick.Away => away ?? bet.Odds,
						_ => bet.Odds
					};
				}


			}

			var jsBetSlip = new
			{
				userId = betSlip.UserId,
				bets = betSlip.Bets.Select(b => new
				{
					matchId = b.MatchId.ToString(),
					homeTeam = b.HomeTeam,
					awayTeam = b.AwayTeam,
					homeLogoUrl = b.HomeLogoUrl,
					awayLogoUrl = b.AwayLogoUrl,
					selectedOption = b.SelectedOption,
					market = b.Market.ToString(),
					selection = b.Selection,
					line = b.Line,
					pickLabel = BetMarketHelper.DisplayPick(b.Market,
						Enum.TryParse<MatchPick>(b.SelectedOption, true, out var p) ? p : default,
						b.Selection, b.Line),
					odds = b.Odds,
					startTimeUtc = b.StartTimeUtc?.ToUniversalTime()
						.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
					isPostponed = b.IsPostponed
				}),
				totalOdds = betSlip.TotalOdds,
				stakeAmount = betSlip.StakeAmount

			};

			return new JsonResult(jsBetSlip);
		}

		[HttpPost]
		public async Task<IActionResult> Add([FromBody] BetSlipItem betDto, [FromServices] MatchFixerDbContext dbContext)
		{
			var match = await dbContext.MatchEvents.FindAsync(betDto.MatchId);

			if (match == null)
				return NotFound($"Match with ID {betDto.MatchId} not found.");

			if (!ModelState.IsValid)
			{
				foreach (var kvp in ModelState)
				{
					foreach (var error in kvp.Value.Errors)
					{
						Console.WriteLine($"{kvp.Key}: {error.ErrorMessage}");
					}
				}
				return BadRequest(ModelState);
			}

			try
			{
				betDto.StartTimeUtc = match.MatchDate;

				await _sessionService.AddBetToSlipAsync(betDto);
				return Ok();
			}
			catch (Exception ex)
			{
				// log ex.Message or ex.ToString()
				return StatusCode(500, new { error = ex.Message });
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Remove([FromBody] RemoveBetRequest item)
		{
			var betSlip = _sessionService.GetBetSlipState() ?? new BetSlipState();

			var betToRemove = betSlip.Bets.FirstOrDefault(b =>
				b.MatchId == item.MatchId &&
				b.SelectedOption == item.SelectedOption);

			if (betToRemove != null)
			{
				betSlip.Bets.Remove(betToRemove);
				_sessionService.SetBetSlipState(betSlip);
			}

			return Ok();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult RemoveAll()
		{
			var betSlip = _sessionService.GetBetSlipState() ?? new BetSlipState();

			betSlip.Bets.Clear();
			_sessionService.SetBetSlipState(betSlip);

			return Ok();
		}


		[HttpGet]
		public async Task<IActionResult> GetEffectiveOdds(Guid matchId, string option, CancellationToken ct, BettingMarket market = BettingMarket.MatchResult)
		{
			var match = await _dbContext.MatchEvents
				.AsNoTracking()
				.FirstOrDefaultAsync(e => e.Id == matchId, ct);

			if (match == null) return NotFound();

			// Extra markets: plain price from MatchOdds (boosts only apply to Match Result)
			if (market != BettingMarket.MatchResult)
			{
				var selection = BetMarketHelper.NormalizeSelection(market, option);
				if (selection == null)
					return BadRequest("Invalid option or odds unavailable.");

				if (!await _limits.IsMarketOfferedAsync(matchId, market, ct))
					return BadRequest("This market is not available for this match.");

				var row = await _dbContext.MatchOdds
					.AsNoTracking()
					.FirstOrDefaultAsync(o => o.MatchEventId == matchId && o.Market == market && o.Selection == selection, ct);

				if (row == null) return BadRequest("Invalid option or odds unavailable.");

				return new JsonResult(new { odds = row.Odds, line = row.Line, selection, boostId = (Guid?)null });
			}

			var (home, draw, away, boost) = await _oddsBoostService.GetEffectiveOddsAsync(
				match.Id,
				match.HomeOdds,
				match.DrawOdds,
				match.AwayOdds,
				ct
			);

			if (!Enum.TryParse<MatchPick>(option, ignoreCase: true, out var pick))
				return BadRequest("Invalid option or odds unavailable.");

			decimal? odds = pick switch
			{
				MatchPick.Home => home,
				MatchPick.Draw => draw,
				MatchPick.Away => away,
				_ => null
			};

			if (odds == null) return BadRequest("Invalid option or odds unavailable.");

			return new JsonResult(new { odds, boostId = boost?.Id });
		}
	}
}		
