using MatchFixer.Infrastructure.Security;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Trophy;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Areas.Admin.Controllers
{
	[Area("Admin")]
	[AdminOnly]
	public class AdminTrophyController : Controller
	{
		private readonly IAdminTrophyService _adminTrophyService;

		public AdminTrophyController(IAdminTrophyService adminTrophyService)
		{
			_adminTrophyService = adminTrophyService;
		}

		[HttpGet]
		public async Task<IActionResult> User(Guid userId)
		{
			var model = await _adminTrophyService.GetUserWithTrophiesAsync(userId);

			if (model == null)
				return NotFound();

			return View("AdminUserTrophies", new List<AdminUserTrophyViewModel> { model });
		}

		public async Task<IActionResult> AdminUserTrophies()
		{
			var model = await _adminTrophyService.GetUsersWithTrophiesAsync();
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ReevaluateUser(Guid userId)
		{
			await _adminTrophyService.ReevaluateUserTrophiesAsync(userId);
			return RedirectToAction(nameof(AdminUserTrophies));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ReevaluateAll()
		{
			await _adminTrophyService.ReevaluateAllUsersAsync();
			return RedirectToAction(nameof(AdminUserTrophies));
		}
	}
}
