using MatchFixer.Infrastructure.Security;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Areas.Admin.Controllers
{
	
	[Area("Admin")]
	[AdminOnly]
	public class DashboardController : Controller
	{
		private readonly IAdminDashboardService _dashboardService;

		public DashboardController(IAdminDashboardService dashboardService)
		{
			_dashboardService = dashboardService;
		}

		[HttpGet("/admin/dashboard")]
		[AdminOnly]
		public async Task<IActionResult> Index()
		{
			var model = await _dashboardService.GetDashboardAsync();
			return View(model);
		}
	}
	
}
