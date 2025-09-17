using MatchFixer.Common.Identity;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static MatchFixer_Web_App.Areas.Admin.ViewModels.Email.EmailBlastCommand;

namespace MatchFixer_Web_App.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = Roles.Admin)]
	public class EmailController : Controller
	{
		private readonly IAdminEmailService _emailService;
		private readonly MatchFixer.Infrastructure.MatchFixerDbContext _db;

		public EmailController(IAdminEmailService emailService, MatchFixer.Infrastructure.MatchFixerDbContext db)
		{
			_emailService = emailService;
			_db = db;
		}

		[HttpGet]
		public async Task<IActionResult> Compose(Guid? userId = null)
		{
			var model = new EmailBlastCommand
			{
				Mode = userId.HasValue ? RecipientMode.Single : RecipientMode.All,
				UserId = userId,
				OnlyConfirmed = true,
				LogoUrl = Url.Content("~/images/logo.png")
			};

			ViewBag.Roles = await _db.Roles
				.AsNoTracking()
				.OrderBy(r => r.Name)
				.Select(r => r.Name)
				.ToListAsync();

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Preview(EmailBlastCommand model)
		{
			if (string.IsNullOrWhiteSpace(model.Subject) || string.IsNullOrWhiteSpace(model.BodyHtml))
			{
				TempData["ErrorMessage"] = "Subject and Body are required.";
				return RedirectToAction(nameof(Compose), new { userId = model.UserId });
			}

			var count = await _emailService.CountRecipientsAsync(model);
			ViewBag.RecipientCount = count;

			ViewBag.PreviewHtml = MatchFixer.Common.EmailTemplates.EmailTemplates
				.BlastTemplate(model.LogoUrl, model.Subject, model.BodyHtml);

			ViewBag.Roles = await _db.Roles.AsNoTracking().OrderBy(r => r.Name).Select(r => r.Name).ToListAsync();

			return View("Compose", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Send(EmailBlastCommand model)
		{
			if (string.IsNullOrWhiteSpace(model.Subject) || string.IsNullOrWhiteSpace(model.BodyHtml))
			{
				TempData["ErrorMessage"] = "Subject and Body are required.";
				return RedirectToAction(nameof(Compose), new { userId = model.UserId });
			}

			var result = await _emailService.SendAsync(model);
			TempData["SuccessMessage"] = $"Queued {result.Sent} of {result.TotalRecipients} (skipped {result.Skipped}).";

			return RedirectToAction("ShowUsers", "Users", new { area = "Admin" });
		}
	}
}
