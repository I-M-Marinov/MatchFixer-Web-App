using MatchFixer.Infrastructure.Security; 
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Users")]
	[AdminOnly]
	public class UsersController : Controller
	{
		private readonly IAdminUserService _svc;
		public UsersController(IAdminUserService svc) => _svc = svc;

		[HttpGet("")]                     
		[HttpGet("[action]")]            
        public async Task<IActionResult> ShowUsers(string? query, int page = 1, int pageSize = 4)
		{
			var vm = await _svc.GetUsersAsync(query, page, pageSize);
			return View(vm); 
		}

		[HttpPost("Lock")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Lock(Guid id)
		{
			var ok = await _svc.LockUserAsync(id);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "User locked." : "Failed to lock user.";
			return RedirectToAction(nameof(ShowUsers));
		}

		[HttpPost("Unlock")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Unlock(Guid id)
		{
			var ok = await _svc.UnlockUserAsync(id);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "User unlocked." : "Failed to unlock user.";
			return RedirectToAction(nameof(ShowUsers));
		}

		[HttpPost("ConfirmEmail")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ConfirmEmail(Guid id)
		{
			var ok = await _svc.MarkEmailConfirmedAsync(id);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Email confirmed." : "Failed to confirm email.";
			return RedirectToAction(nameof(ShowUsers));
		}

		[HttpPost("ResetPasswordLink")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPasswordLink(Guid id)
		{
			var (ok, link) = await _svc.GenerateResetPasswordLinkAsync(id, Url);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? $"Reset link created: {link}" : "Failed to generate link.";
			return RedirectToAction(nameof(ShowUsers));
		}

		[HttpPost("AddRole")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddRole(Guid id, string role)
		{
			var ok = await _svc.AddRoleAsync(id, role);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? $"Role '{role}' added." : "Failed to add role.";
			return RedirectToAction(nameof(ShowUsers));
		}

		[HttpPost("RemoveRole")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RemoveRole(Guid id, string role)
		{
			var ok = await _svc.RemoveRoleAsync(id, role);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? $"Role '{role}' removed." : "Failed to remove role.";
			return RedirectToAction(nameof(ShowUsers));
		}
	}
}
