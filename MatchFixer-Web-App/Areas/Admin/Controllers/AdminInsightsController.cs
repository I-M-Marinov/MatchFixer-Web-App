using MatchFixer.Infrastructure.Security;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
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
		public async Task<IActionResult> EventsSpread(int page = 1, CancellationToken ct = default)
		{
			var model = await _svc.GetUpcomingEventBetStatsAsync(page, PageSize, ct);
			return View(model);
		}
	}
}
