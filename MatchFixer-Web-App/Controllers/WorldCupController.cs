using MatchFixer.Core.Contracts;
using MatchFixer.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
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

		[HttpGet]
		[AdminOnly]
		public async Task<IActionResult> BracketReorder()
		{
			var matches = await _worldCupService.GetKnockoutMatchOrderAsync();
			return View(matches);
		}

		[HttpPost]
		[AdminOnly]
		public async Task<IActionResult> SaveBracketOrder()
		{
			using var reader = new StreamReader(Request.Body);
			var body = await reader.ReadToEndAsync();

			List<BracketOrderItem>? items;
			try
			{
				items = JsonSerializer.Deserialize<List<BracketOrderItem>>(
					body,
					new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
			}
			catch
			{
				return BadRequest(new { success = false, message = "Invalid payload." });
			}

			if (items == null || items.Count == 0)
				return BadRequest(new { success = false, message = "No items received." });

			var order = items.Select(x => (x.Id, x.Position));
			await _worldCupService.SaveBracketOrderAsync(order);

			return Ok(new { success = true, message = $"{items.Count} positions saved." });
		}

		private sealed record BracketOrderItem(int Id, int Position);
	}
}
