using MatchFixer.Common.EmailTemplates;
using MatchFixer.Infrastructure;
using static MatchFixer.Common.Admin.AdminUserServiceConstants;
using MatchFixer.Infrastructure.Entities;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using static MatchFixer_Web_App.Areas.Admin.ViewModels.Email.EmailBlastCommand;

namespace MatchFixer_Web_App.Areas.Admin.Services
{
	public class AdminEmailService : IAdminEmailService
	{
		private readonly MatchFixerDbContext _db;
		private readonly IEmailSender _email;
		private readonly IHttpContextAccessor _http;

		public AdminEmailService(MatchFixerDbContext db, IEmailSender email, IHttpContextAccessor http)
		{
			_db   = db;
			_email = email;
			_http  = http;
		}

		// Builds an absolute base URL (scheme + host) 
		private string BaseUrl()
		{
			var req = _http.HttpContext?.Request;
			return req is null ? "" : $"{req.Scheme}://{req.Host}";
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
				// Never email deleted accounts unless status == StatusDeleted
				if (r.IsDeleted && !string.Equals(cmd.Status, StatusDeleted, StringComparison.OrdinalIgnoreCase))
				{
					skipped++;
					continue;
				}

				await _email.SendEmailAsync(r.Email, cmd.Subject, html);
				sent++;
			}

			return new EmailBlastResult(recipients.Count, sent, skipped);
		}


		public async Task<List<EmailTemplateDto>> GetEmailTemplatesAsync()
		{
			var baseUrl = BaseUrl();

			var templates = new List<EmailTemplateDto>
			{
				new("welcome_back",  "Welcome Back Promotion",          "We Miss You – Come Back and Bet!",              EmailTemplates.BlastBodyWelcomeBack($"{baseUrl}/")),
				new("world_cup",     "World Cup Special",               "🌍 World Cup Bets Are Live – Place Yours Now!", EmailTemplates.BlastBodyWorldCup($"{baseUrl}/WorldCup/WorldCup")),
				new("weekend_promo", "Weekend Promo",                   "⚽ Big Weekend Ahead – Don't Miss It!",          EmailTemplates.BlastBodyWeekend($"{baseUrl}/")),
			};

			// Boosted-matches template — only when at least one active boost exists
			var now = DateTime.UtcNow;
			var activeBoosts = await _db.OddsBoosts
				.AsNoTracking()
				.Include(b => b.MatchEvent).ThenInclude(m => m.HomeTeam)
				.Include(b => b.MatchEvent).ThenInclude(m => m.AwayTeam)
				.Where(b => b.IsActive && b.StartUtc <= now && b.EndUtc >= now && !b.MatchEvent.IsCancelled)
				.OrderBy(b => b.MatchEvent.MatchDate)
				.ToListAsync();

			if (activeBoosts.Any())
			{
				var boostData = activeBoosts.Select(b => (
					Home:  b.MatchEvent.HomeTeam.Name,
					Away:  b.MatchEvent.AwayTeam.Name,
					Boost: $"+{b.BoostValue:0.00}",
					Until: b.EndUtc.ToString("dd MMM, HH:mm")
				));

				templates.Insert(0, new EmailTemplateDto(
					Id:      "boosted_matches",
					Label:   "🔥 Boosted Matches (Active Now)",
					Subject: "🔥 Boosted Odds Alert – Limited Time Only!",
					Body:    EmailTemplates.BlastBodyBoostedMatches(boostData, $"{baseUrl}/")
				));
			}

			return templates;
		}

		private IQueryable<ApplicationUser> BuildRecipientsQuery(EmailBlastCommand cmd)
		{
			var users = _db.Users.AsNoTracking().AsQueryable();

			// Status filter (mirror your Admin Users list)
			if (!string.IsNullOrEmpty(cmd.Status))
			{
				switch (cmd.Status.ToLowerInvariant())
				{
					case StatusActive:
						users = users.Where(u => u.IsActive && !u.IsDeleted && u.LockoutEnd == null);
						break;
					case StatusUnconfirmed:
						users = users.Where(u => !u.EmailConfirmed && !u.IsDeleted);
						break;
					case StatusLocked:
						users = users.Where(u => u.LockoutEnd != null && !u.IsDeleted);
						break;
					case StatusDeleted:
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

			// Keep counts consistent with Send: only confirmed + has email
			if (cmd.OnlyConfirmed)
				users = users.Where(u => u.EmailConfirmed);

			users = users.Where(u => !string.IsNullOrEmpty(u.Email));

			return users;
		}

}
}
