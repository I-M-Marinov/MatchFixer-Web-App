using MatchFixer.Infrastructure.Security;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Areas.Admin.Controllers
{
	[Area("Admin")]
	[AdminOnly]
	public sealed class AdminInsightsController : Controller
	{
		private readonly IAdminBetInsightsService _svc;
		private const int PageSize = 20;

		public AdminInsightsController(IAdminBetInsightsService svc) => _svc = svc;

		[HttpGet]
		public async Task<IActionResult> EventsSpread(
			string? league,
			int page = 1,
			EventSort sort = EventSort.TotalBets,
			bool desc = true,
			CancellationToken ct = default)
		{
			var model = await _svc.GetUpcomingEventBetStatsAsync(
				league,
				page,
				PageSize,
				sort,
				desc,
				ct);

			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> EventsSpreadRows(
			string? league,
			int page = 1,
			int pageSize = 10,
			EventSort sort = EventSort.TotalBets,
			bool desc = false,
			CancellationToken ct = default)
		{
			var model = await _svc.GetUpcomingEventBetStatsAsync(
				league,
				page,
				pageSize,
				sort,
				desc,
				ct);

			return PartialView("_EventsSpreadRows", model.Items);
		}


	}
}
