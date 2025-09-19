using MatchFixer.Common.EmailTemplates;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Email;
using Microsoft.AspNetCore.Identity.UI.Services; // if you use IEmailSender
using Microsoft.EntityFrameworkCore;
using static MatchFixer_Web_App.Areas.Admin.ViewModels.Email.EmailBlastCommand;

namespace MatchFixer_Web_App.Areas.Admin.Services
{
	public class AdminEmailService : IAdminEmailService
	{
		private readonly MatchFixerDbContext _db;
		private readonly IEmailSender _email; // or your own abstraction

		public AdminEmailService(MatchFixerDbContext db, IEmailSender email)
		{
			_db = db;
			_email = email;
		}

		public async Task<int> CountRecipientsAsync(EmailBlastCommand cmd)
		{
			var query = BuildRecipientsQuery(cmd);
			return await query.CountAsync();
		}

		public async Task<EmailBlastResult> SendAsync(EmailBlastCommand cmd)
		{
			var recipients = await BuildRecipientsQuery(cmd)
				.Select(u => new { u.Email, u.EmailConfirmed, u.IsDeleted })
				.ToListAsync();

			var html = EmailTemplates.BlastTemplate(cmd.Subject, cmd.BodyHtml);

			int sent = 0, skipped = 0;

			foreach (var r in recipients)
			{
				if (string.IsNullOrWhiteSpace(r.Email))
				{
					skipped++;
					continue;
				}
				if (cmd.OnlyConfirmed && !r.EmailConfirmed)
				{
					skipped++;
					continue;
				}
				// Never email deleted accounts unless status == "deleted"
				if (r.IsDeleted && !string.Equals(cmd.Status, "deleted", StringComparison.OrdinalIgnoreCase))
				{
					skipped++;
					continue;
				}

				await _email.SendEmailAsync(r.Email, cmd.Subject, html);
				sent++;
			}

			return new EmailBlastResult(recipients.Count, sent, skipped);
		}


		private IQueryable<ApplicationUser> BuildRecipientsQuery(EmailBlastCommand cmd)
		{
			var users = _db.Users.AsNoTracking().AsQueryable();

			// Status filter (mirror your Admin Users list)
			if (!string.IsNullOrEmpty(cmd.Status))
			{
				switch (cmd.Status.ToLowerInvariant())
				{
					case "active":
						users = users.Where(u => u.IsActive && !u.IsDeleted && u.LockoutEnd == null);
						break;
					case "unconfirmed":
						users = users.Where(u => !u.EmailConfirmed && !u.IsDeleted);
						break;
					case "locked":
						users = users.Where(u => u.LockoutEnd != null && !u.IsDeleted);
						break;
					case "deleted":
						users = users.Where(u => u.IsDeleted);
						break;
				}
			}
			else
			{
				users = users.Where(u => !u.IsDeleted);
			}

			// Mode filter
			switch (cmd.Mode)
			{
				case RecipientMode.Single:
					if (cmd.UserId.HasValue)
						users = users.Where(u => u.Id == cmd.UserId.Value);
					break;

				case RecipientMode.Role:
					if (!string.IsNullOrWhiteSpace(cmd.Role))
					{
						var roleUsers =
							from ur in _db.UserRoles
							join r in _db.Roles on ur.RoleId equals r.Id
							where r.Name == cmd.Role
							select ur.UserId;

						users = users.Where(u => roleUsers.Contains(u.Id));
					}
					break;

				case RecipientMode.All:
				default:
					break;
			}

			// Keep counts consistent with Send: only confirmed + has email (optional, but recommended)
			if (cmd.OnlyConfirmed)
				users = users.Where(u => u.EmailConfirmed);

			users = users.Where(u => !string.IsNullOrEmpty(u.Email));

			return users;
		}

}
}
