using MatchFixer.Infrastructure.Security;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Events;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Areas.Admin.Controllers
{
	[Area("Admin")]
	[AdminOnly]
	public class AdminEventsController : Controller
	{
		private readonly IAdminEventsService _adminEventsService;

		public AdminEventsController(IAdminEventsService adminEventsService)
		{
			_adminEventsService = adminEventsService;
		}

		[HttpGet]
		public async Task<IActionResult> History(AdminEventHistoryFilters filters)
		{
			// Bound the query: default to last 30 days when no date filter is provided
			filters.FromDate ??= DateTime.Today.AddDays(-30);
			filters.ToDate   ??= DateTime.Today;

			var pagedEvents = await _adminEventsService.GetFinishedEventsAsync(filters);
			var teamStats   = await _adminEventsService.GetTeamBettingStatsAsync(filters);

			ViewBag.TeamStats = teamStats;
			ViewBag.Filters   = filters;

			return View(pagedEvents);
		}
	}
}
