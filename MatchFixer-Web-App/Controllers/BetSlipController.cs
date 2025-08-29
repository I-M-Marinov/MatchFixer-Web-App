using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Bets;
using MatchFixer.Core.Services;
using MatchFixer.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace MatchFixer_Web_App.Controllers
{
	[ApiController]
	[Route("[controller]/[action]")]
	public class BetSlipController : ControllerBase
	{
		private readonly ISessionService _sessionService;
		private readonly MatchFixerDbContext _dbContext;
		private readonly IOddsBoostService _oddsBoostService;

		public BetSlipController(ISessionService sessionService, MatchFixerDbContext dbContext,
			IOddsBoostService oddsBoostService)
		{
			_sessionService = sessionService;
			_dbContext = dbContext;
			_oddsBoostService = oddsBoostService;
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

			// Update session bets with latest effective odds (boosts respected)
			foreach (var bet in betSlip.Bets)
			{
				if (!matches.TryGetValue(bet.MatchId, out var match))
					continue;

				var (home, draw, away, _) = await oddsBoostService.GetEffectiveOddsAsync(
					match.Id,
					match.HomeOdds,
					match.DrawOdds,
					match.AwayOdds,
					ct: default
				);

				// Only update odds if effective odds exist, otherwise keep current session odds
				bet.Odds = bet.SelectedOption switch
				{
					"Home" => home ?? bet.Odds,
					"Draw" => draw ?? bet.Odds,
					"Away" => away ?? bet.Odds,
					_ => bet.Odds
				};


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
					odds = b.Odds,
					startTimeUtc = b.StartTimeUtc?.ToUniversalTime()
						.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
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
		public async Task<IActionResult> GetEffectiveOdds(Guid matchId, string option, CancellationToken ct)
		{
			var match = await _dbContext.MatchEvents
				.AsNoTracking()
				.FirstOrDefaultAsync(e => e.Id == matchId, ct);

			if (match == null) return NotFound();

			var (home, draw, away, boost) = await _oddsBoostService.GetEffectiveOddsAsync(
				match.Id,
				match.HomeOdds,
				match.DrawOdds,
				match.AwayOdds,
				ct
			);

			decimal? odds = option switch
			{
				"Home" => home,
				"Draw" => draw,
				"Away" => away,
				_ => null
			};

			if (odds == null) return BadRequest("Invalid option or odds unavailable.");

			return new JsonResult(new { odds, boostId = boost?.Id });
		}
	}
}		
