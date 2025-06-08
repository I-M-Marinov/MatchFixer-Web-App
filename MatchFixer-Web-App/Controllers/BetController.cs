using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MatchFixer.Core.DTOs.Bets;
using MatchFixer.Core.Contracts;

namespace MatchFixer_Web_App.Controllers
{
	public class BetController : Controller
	{

		private readonly IBettingService _bettingService;

		public BetController(IBettingService bettingService)
		{
			_bettingService = bettingService;
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

			TempData["SuccessMessage"] = message;
			return RedirectToAction("LiveEvents", "Event");
		}

	}
}
