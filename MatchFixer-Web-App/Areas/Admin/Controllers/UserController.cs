using MatchFixer.Infrastructure.Security; 
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using MatchFixer.Common.GeneralConstants;
using MatchFixer.Common.Admin;
namespace MatchFixer_Web_App.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("admin/users")]
	[AdminOnly]
	public class UsersController : Controller
	{
		private readonly IAdminUserService _svc;
		public UsersController(IAdminUserService svc) => _svc = svc;

		[HttpGet("/admin/users", Name = "AdminUsersList")]
		public async Task<IActionResult> ShowUsers(string? query, string? status, int page = 1, int pageSize = 5)
		{

			// Default to the "active" filter without redirecting, so /admin/users
			// renders directly (no self-redirect that could mis-generate a URL).
			if (string.IsNullOrWhiteSpace(status))
			{
				status = AdminUserServiceConstants.StatusActive;
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
				TempData[TempDataKeys.ErrorMessage] = "Unable to resolve your user id.";
				return RedirectToAction(nameof(ShowUsers));
			}

			var (ok, msg) = await _svc.LockUserAsync(actorId, id);
			TempData[ok ? TempDataKeys.SuccessMessage : TempDataKeys.ErrorMessage] = msg;
			return RedirectToAction(nameof(ShowUsers));
		}

		[HttpPost("Unlock")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Unlock(Guid id)
		{
			var me = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!Guid.TryParse(me, out var actorId))
			{
				TempData[TempDataKeys.ErrorMessage] = "Unable to resolve your user id.";
				return RedirectToAction(nameof(ShowUsers));
			}

			var (ok, msg) = await _svc.UnlockUserAsync(actorId, id);
			TempData[ok ? TempDataKeys.SuccessMessage : TempDataKeys.ErrorMessage] = msg;
			return RedirectToAction(nameof(ShowUsers));
		}

		[HttpPost("ConfirmEmail")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ConfirmEmail(Guid id)
		{
			var ok = await _svc.MarkEmailConfirmedAsync(id);
			TempData[ok ? TempDataKeys.SuccessMessage : TempDataKeys.ErrorMessage] = ok ? "Email confirmed." : "Failed to confirm email.";
			return RedirectToAction(nameof(ShowUsers));
		}

		[HttpPost("ResetPasswordLink")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPasswordLink(Guid id)
		{
			var (ok, link) = await _svc.GenerateResetPasswordLinkAsync(id, Url);
			TempData[ok ? TempDataKeys.SuccessMessage : TempDataKeys.ErrorMessage] = ok ? $"Reset link created: {link}" : "Failed to generate link.";
			return RedirectToAction(nameof(ShowUsers));
		}

		[HttpPost("AddRole")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddRole(Guid id, string role)
		{
			var ok = await _svc.AddRoleAsync(id, role);
			TempData[ok ? TempDataKeys.SuccessMessage : TempDataKeys.ErrorMessage] = ok ? $"Role '{role}' added." : "Failed to add role.";
			return RedirectToAction(nameof(ShowUsers));
		}

		[HttpPost("RemoveRole")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RemoveRole(Guid id, string role)
		{
			var ok = await _svc.RemoveRoleAsync(id, role);
			TempData[ok ? TempDataKeys.SuccessMessage : TempDataKeys.ErrorMessage] = ok ? $"Role '{role}' removed." : "Failed to remove role.";
			return RedirectToAction(nameof(ShowUsers));
		}
	}
}
