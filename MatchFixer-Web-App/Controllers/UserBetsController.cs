using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using static MatchFixer.Common.GeneralConstants.UserBetsConstants;

using MatchFixer.Common.GeneralConstants;
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

		[HttpGet]
		public async Task<IActionResult> UserBets()
		{
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!Guid.TryParse(userIdString, out var userId))
			{
				return Unauthorized();
			}

			var model = await _bettingService.GetBetSlipsPageAsync(userId, nameof(BetStatus.Pending), 1, 15);

			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> Slips(string status = nameof(BetStatus.Pending), int page = 1, int pageSize = 15)
		{
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!Guid.TryParse(userIdString, out var userId))
			{
				return Unauthorized();
			}

			// Only allow the page sizes offered in the UI dropdown.
			if (pageSize != 15 && pageSize != 25 && pageSize != 50)
			{
				pageSize = 15;
			}

			var model = await _bettingService.GetBetSlipsPageAsync(userId, status, page, pageSize);

			return PartialView("_BetSlipList", model);
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
				TempData[TempDataKeys.ErrorMessage] = CouldNotEvaluateBetslip;
			}
			else
			{
				TempData[TempDataKeys.SuccessMessage] = BetSlipEvaluatedSuccessfully;
			}

			// Redirect back to where the request came from
			var redirectUrl = Request.Headers["Referer"].ToString();

			return Redirect(redirectUrl);
		}

	}
}
