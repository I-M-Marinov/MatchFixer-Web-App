using MatchFixer.Core.Contracts;
using MatchFixer.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using static MatchFixer.Common.GeneralConstants.WorldCupApiConstants;

namespace MatchFixer_Web_App.Controllers
{
	public class WorldCupController : Controller
	{
		private readonly IWorldCupService _worldCupService;

		public WorldCupController(IWorldCupService worldCupService)
		{
			_worldCupService = worldCupService;
		}

		public async Task<IActionResult> WorldCup()
		{
			var model = await _worldCupService.GetWorldCupPageAsync();

			return View(model);
		}

		[HttpPost]
		[AdminOnly]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RefreshKnockoutStage()
		{
			var updated = await _worldCupService.RefreshKnockoutStageAsync();

			TempData[TempDataRefreshMessage] =
				updated > 0
					? string.Format(MsgKnockoutRefreshed, updated)
					: MsgKnockoutNoData;

			return RedirectToAction(nameof(WorldCup));
		}

		[HttpPost]
		[AdminOnly]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ReclassifyAndRefresh()
		{
			var reseeded = await _worldCupService.ReclassifyAndRefreshAsync();

			TempData[TempDataRefreshMessage] =
				reseeded > 0
					? string.Format(MsgReclassifyRefreshed, reseeded)
					: MsgReclassifyNoData;

			return RedirectToAction(nameof(WorldCup));
		}

		[HttpPost]
		[AdminOnly]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RefreshGroupStandings()
		{
			var count = await _worldCupService.RefreshGroupStandingsAsync();

			TempData[TempDataRefreshMessage] =
				count > 0
					? string.Format(MsgStandingsRefreshed, count)
					: MsgStandingsNoData;

			return RedirectToAction(nameof(WorldCup));
		}
	}
}
