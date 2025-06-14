using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Bets;
using MatchFixer.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MatchFixer_Web_App.Controllers
{
	public class BetController : Controller
	{

		private readonly IBettingService _bettingService;
		private readonly ISessionService _sessionService;

		public BetController(IBettingService bettingService, ISessionService sessionService)
		{
			_bettingService = bettingService;
			_sessionService = sessionService;

		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> PlaceBet(BetSlipDto model)
		{
			var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var (message, success) = await _bettingService.PlaceBetAsync(userId, model);

			if (!success)
			{
				TempData["ErrorMessage"] = message;
				return RedirectToAction("LiveEvents", "Event");
			}

			_sessionService.ClearBetSlip();

			TempData["SuccessMessage"] = message;
			return RedirectToAction("LiveEvents", "Event");
		}

	}
}
