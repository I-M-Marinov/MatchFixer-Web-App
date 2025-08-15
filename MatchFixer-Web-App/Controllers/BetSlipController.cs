using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Bets;
using Microsoft.AspNetCore.Mvc;
using MatchFixer.Infrastructure;
using Microsoft.EntityFrameworkCore;


namespace MatchFixer_Web_App.Controllers
{
	[ApiController]
	[Route("[controller]/[action]")]
	public class BetSlipController : ControllerBase
	{
		private readonly ISessionService _sessionService;

		public BetSlipController(ISessionService sessionService)
		{
			_sessionService = sessionService;
		}

		[HttpGet]
		public async Task<IActionResult> Get([FromServices] MatchFixerDbContext dbContext)
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

			// Fetch latest odds for all matches in the bet slip
			var matchIds = betSlip.Bets.Select(b => b.MatchId).ToList();
			var matches = await dbContext.MatchEvents
				.Where(m => matchIds.Contains(m.Id))
				.ToDictionaryAsync(m => m.Id);

			// Update session bets with latest odds
			foreach (var bet in betSlip.Bets)
			{
				if (matches.TryGetValue(bet.MatchId, out var match))
				{
					switch (bet.SelectedOption)
					{
						case "Home": bet.Odds = (decimal)match.HomeOdds; break;
						case "Draw": bet.Odds = (decimal)match.DrawOdds; break;
						case "Away": bet.Odds = (decimal)match.AwayOdds; break;
					}
				}
			}

			// Save updated session
			_sessionService.SetBetSlipState(betSlip);

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
					option = b.SelectedOption,
					odds = b.Odds,
					startTimeUtc = b.StartTimeUtc.ToString("o")
				}),
				totalOdds = betSlip.TotalOdds,
				stakeAmount = betSlip.StakeAmount
			};

			return new JsonResult(jsBetSlip);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Add([FromBody] BetSlipItem betDto, [FromServices] MatchFixerDbContext dbContext)
		{
			if (betDto == null)
				return BadRequest("Invalid bet data.");

			var match = await dbContext.MatchEvents.FindAsync(betDto.MatchId);

			if (match == null)
				return NotFound("Match not found.");

			betDto.StartTimeUtc = match.MatchDate;
			_sessionService.AddBetToSlip(betDto);

			return Ok();
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

	}
}
