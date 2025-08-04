using MatchFixer.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Controllers
{
	public class TrophyController : Controller
	{
		private readonly IUserContextService _userContextService;
		private readonly ITrophyService _trophyService;


		public TrophyController(IUserContextService userContextService, ITrophyService trophyService)
		{
			_userContextService = userContextService;
			_trophyService = trophyService;
		}

		[Authorize]
		[HttpGet]
		public async Task<IActionResult> TrophyCatalog()
		{
			var userId = _userContextService.GetUserId();
			if (userId == Guid.Empty)
				return Unauthorized();

			var allTrophies = await _trophyService.GetAllTrophiesWithUserStatusAsync(userId);
			return View(allTrophies); 
		}


		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> MarkAsSeen(int trophyId)
		{
			var userId = _userContextService.GetUserId();
			if (userId == Guid.Empty)
				return Unauthorized();

			await _trophyService.MarkTrophyAsSeenAsync(userId, trophyId);
			return Ok();
		}

	}
}
