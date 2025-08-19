using MatchFixer.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using static MatchFixer.Common.GeneralConstants.UserBetsConstants;

namespace MatchFixer_Web_App.Controllers
{
	[Authorize]
	public class UserBetsController : Controller
	{

		private readonly IBettingService _bettingService;

		public UserBetsController(IBettingService bettingService)
		{
			_bettingService = bettingService;
		}

		public async Task<IActionResult> UserBets()
		{
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!Guid.TryParse(userIdString, out var userId))
			{
				return Unauthorized();
			}

			var userBets = await _bettingService.GetBetsByUserAsync(userId);

			return View(userBets);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EvaluateSlip(Guid betSlipId)
		{
			if (betSlipId == Guid.Empty)
				return BadRequest();

			var success = await _bettingService.EvaluateBetSlipAsync(betSlipId);

			if (!success)
			{
				TempData["ErrorMessage"] = CouldNotEvaluateBetslip;
			}
			else
			{
				TempData["SuccessMessage"] = BetSlipEvaluatedSuccessfully;
			}

			return RedirectToAction(nameof(UserBets));
		}

	}
}
