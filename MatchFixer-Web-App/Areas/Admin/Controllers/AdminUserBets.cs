using MatchFixer.Common.Identity;
using MatchFixer.Core.Contracts;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = Roles.Admin)]
	public class AdminUserBetsController : Controller
	{
		private readonly IAdminUserBetsService _svc;

		public AdminUserBetsController(IAdminUserBetsService svc) => _svc = svc;

		[HttpGet]
		public async Task<IActionResult> Details(Guid userId)
		{
			var tzId = Request.Cookies.TryGetValue("tz", out var v) ? v : "Europe/Sofia";
			var vm = await _svc.GetColumnsAsync(userId, tzId, maxPerColumn: 200);
			if (vm is null)
			{
				TempData["ErrorMessage"] = "User not found.";
				return RedirectToAction("ShowUsers", "Users");
			}
			return View(vm);
		}
	}
}
