using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Bets;
using Microsoft.AspNetCore.Mvc;


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
		public IActionResult Get()
		{

			var betSlip = _sessionService.GetBetSlipState();

			if (betSlip == null)
			{
				return new JsonResult(new
				{
					userId = (string?)null,
					bets = new object[] { },
					totalOdds = 0.0,
					stakeAmount = 0.0
				});
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
					option = b.SelectedOption,
					odds = b.Odds
				}),
				totalOdds = betSlip.TotalOdds,
				stakeAmount = betSlip.StakeAmount
			};

			return new JsonResult(jsBetSlip);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Add([FromBody] BetSlipItem betDto)
		{
			if (betDto == null)
				return BadRequest("Invalid bet data.");

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

	}
}
