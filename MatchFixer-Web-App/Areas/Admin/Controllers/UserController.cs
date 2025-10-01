using MatchFixer.Infrastructure.Security; 
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
		public async Task<IActionResult> ShowUsers(string? query, string? status, int page = 1, int pageSize = 5)
		{

			if (string.IsNullOrWhiteSpace(status))
			{
				return RedirectToAction(nameof(ShowUsers), new { query, status = "active", page, pageSize });
			}
			
			var vm = await _svc.GetUsersAsync(query, status, page, pageSize);
			return View(vm);

		}

		[HttpPost("Lock")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Lock(Guid id)
		{
			var me = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!Guid.TryParse(me, out var actorId))
			{
				TempData["ErrorMessage"] = "Unable to resolve your user id.";
				return RedirectToAction(nameof(ShowUsers));
			}

			var (ok, msg) = await _svc.LockUserAsync(actorId, id);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = msg;
			return RedirectToAction(nameof(ShowUsers));
		}

		[HttpPost("Unlock")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Unlock(Guid id)
		{
			var me = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!Guid.TryParse(me, out var actorId))
			{
				TempData["ErrorMessage"] = "Unable to resolve your user id.";
				return RedirectToAction(nameof(ShowUsers));
			}

			var (ok, msg) = await _svc.UnlockUserAsync(actorId, id);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = msg;
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
