using MatchFixer.Common.EmailTemplates;
using MatchFixer.Core.Contracts;
using MatchFixer.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

using static MatchFixer.Common.GeneralConstants.ProfilePictureConstants;


namespace MatchFixer.Core.Services
{
	public class NotificationService : INotificationService
	{
		private readonly MatchFixerDbContext _db;
		private readonly IEmailSender _emailSender;
		private readonly IHttpContextAccessor _httpContextAccessor;




		public NotificationService(MatchFixerDbContext db, IEmailSender emailSender, IHttpContextAccessor httpContextAccessor)
		{
			_db = db;
			_emailSender = emailSender;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task NotifyUsersForMatchAsync(Guid matchId)
		{
			var match = await _db.MatchEvents
				.Include(x => x.HomeTeam)
				.Include(x => x.AwayTeam)
				.FirstOrDefaultAsync(x => x.Id == matchId);

			if (match == null)
			{
				Console.WriteLine("MATCH IS NULL");
				return;
			}

			var teamIds = new[] { match.HomeTeamId, match.AwayTeamId };

			var users = await _db.UserFavoriteTeams
				.Where(x => teamIds.Contains(x.TeamId))
				.Include(x => x.User)
				.Select(x => new
				{
					Email = x.User.Email,
					TeamId = x.TeamId
				})
				.Where(x => !string.IsNullOrEmpty(x.Email))
				.Distinct()
				.ToListAsync();

			Console.WriteLine($"Users to notify: {users.Count}");

			if (!users.Any())
			{
				Console.WriteLine("NO USERS FOUND FOR THIS MATCH");
				return;
			}

			var request = _httpContextAccessor.HttpContext?.Request;

			var baseUrl = request != null
				? $"{request.Scheme}://{request.Host}"
				: "https://fallback-domain.bg";

			var emailTasks = users.Select(u =>
			{
				var teamName = u.TeamId == match.HomeTeamId
					? match.HomeTeam.Name
					: match.AwayTeam.Name;

				var callbackUrl = $"{baseUrl}/Event/LiveEvents?team={Uri.EscapeDataString(teamName)}";

				return _emailSender.SendEmailAsync(
					u.Email!,
					$"⚽ {match.HomeTeam.Name} vs {match.AwayTeam.Name}",
					EmailTemplates.MatchAddedEmail(
						logoUrl: LogoUrl,
						homeTeam: match.HomeTeam.Name,
						awayTeam: match.AwayTeam.Name,
						matchTime: match.MatchDate.HasValue
							? match.MatchDate.Value.ToString("f")
							: "TBD",
						link: callbackUrl
					)
				);
			});

			await Task.WhenAll(emailTasks);

			Console.WriteLine("EMAILS SENT");
		}
	}
}
