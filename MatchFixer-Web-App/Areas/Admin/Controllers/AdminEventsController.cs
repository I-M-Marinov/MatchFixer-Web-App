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
			var events = await _adminEventsService.GetFinishedEventsAsync(filters);
			var teamStats = await _adminEventsService.GetTeamBettingStatsAsync(filters);

			ViewBag.TeamStats = teamStats;

			return View(events);
		}
	}
}
