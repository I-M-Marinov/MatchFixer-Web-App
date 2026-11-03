using System.Security.Claims;
using MatchFixer.Common.GeneralConstants;
using MatchFixer.Core.Contracts;
using MatchFixer.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Areas.Admin.Controllers
{
	[Area("Admin")]
	[AdminOnly]
	public class ChangeRequestController : Controller
	{
		private readonly IChangeRequestService _service;

		public ChangeRequestController(IChangeRequestService service)
		{
			_service = service;
		}

		private Guid AdminId =>
			Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Approve(Guid id, string? note)
		{
			var (ok, msg) = await _service.ApproveAsync(id, AdminId, note);
			TempData[ok ? TempDataKeys.SuccessMessage : TempDataKeys.ErrorMessage] = msg;
			return RedirectToAction("Index", "EmailQueue", new { area = "Admin", tab = "approvals" });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Reject(Guid id, string? note)
		{
			var (ok, msg) = await _service.RejectAsync(id, AdminId, note);
			TempData[ok ? TempDataKeys.SuccessMessage : TempDataKeys.ErrorMessage] = msg;
			return RedirectToAction("Index", "EmailQueue", new { area = "Admin", tab = "approvals" });
		}
	}
}
