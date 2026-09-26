using System.Security.Claims;
using MatchFixer.Common.Enums;
using MatchFixer.Common.GeneralConstants;
using MatchFixer.Core.Contracts;
using MatchFixer.Infrastructure.Security;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Email;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Areas.Admin.Controllers
{
	[Area("Admin")]
	[AdminOnly]
	public class EmailQueueController : Controller
	{
		private readonly IEmailQueueService _queue;

		public EmailQueueController(IEmailQueueService queue)
		{
			_queue = queue;
		}

		private Guid AdminId =>
			Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var all = await _queue.GetReviewQueueAsync();

			var model = new EmailQueueViewModel
			{
				Winning = all.Where(x => x.Category == EmailQueueCategory.Winning).ToList(),
				Other = all.Where(x => x.Category != EmailQueueCategory.Winning).ToList(),
				PendingCount = await _queue.GetPendingCountAsync()
			};

			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> Get(Guid id)
		{
			var email = await _queue.GetAsync(id);
			if (email == null)
				return NotFound();

			return Json(new
			{
				id = email.Id,
				subject = email.Subject,
				bodyHtml = email.BodyHtml,
				recipient = email.Recipient,
				status = email.Status.ToString()
			});
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Update(Guid id, string subject, string bodyHtml)
		{
			var ok = await _queue.UpdateContentAsync(id, subject, bodyHtml);

			if (ok)
				TempData[TempDataKeys.SuccessMessage] = "Email updated.";
			else
				TempData[TempDataKeys.ErrorMessage] = "Email not found or already sent.";

			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Drop(Guid[] ids)
		{
			var n = await _queue.DropAsync(ids ?? Array.Empty<Guid>(), AdminId);
			TempData[TempDataKeys.SuccessMessage] = $"Dropped {n} email(s).";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SendNow(Guid[] ids)
		{
			var n = await _queue.SendNowAsync(ids ?? Array.Empty<Guid>(), AdminId);
			TempData[TempDataKeys.SuccessMessage] = $"Sent {n} email(s).";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Schedule(Guid[] ids, int intervalSeconds)
		{
			var n = await _queue.ScheduleAsync(ids ?? Array.Empty<Guid>(), intervalSeconds, AdminId);
			TempData[TempDataKeys.SuccessMessage] = n > 0
				? $"Scheduled {n} email(s) to send {intervalSeconds}s apart."
				: "No eligible emails were scheduled.";
			return RedirectToAction(nameof(Index));
		}
	}
}
