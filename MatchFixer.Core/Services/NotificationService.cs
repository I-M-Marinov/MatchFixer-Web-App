using MatchFixer.Common.EmailTemplates;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.UserNotifications;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
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
		private readonly ITimezoneService _timezoneService;





		public NotificationService(MatchFixerDbContext db, IEmailSender emailSender, IHttpContextAccessor httpContextAccessor, ITimezoneService timezoneService)
		{
			_db = db;
			_emailSender = emailSender;
			_httpContextAccessor = httpContextAccessor;
			_timezoneService = timezoneService;
		}

		public async Task NotifyUsersForMatchAsync(Guid matchId)
		{
			var match = await _db.MatchEvents
				.Include(x => x.HomeTeam)
				.ThenInclude(t => t.WikiInfo)
				.Include(x => x.AwayTeam)
				.ThenInclude(t => t.WikiInfo)
				.FirstOrDefaultAsync(x => x.Id == matchId);

			if (match == null)
			{
				return;
			}

			var teamIds = new[] { match.HomeTeamId, match.AwayTeamId };

			var users = await _db.UserFavoriteTeams
				.Where(x => teamIds.Contains(x.TeamId))
				.Include(x => x.User)
				.Where(x => !string.IsNullOrEmpty(x.User.Email))
				.GroupBy(x => x.User.Email)
				.Select(g => new UserNotificationDto
				{
					Email = g.Key,
					TeamId = g.First().TeamId,
					TimeZone = g.First().User.TimeZone
				})
				.ToListAsync();

			if (!users.Any())
			{
				return;
			}

			var request = _httpContextAccessor.HttpContext?.Request;

			var baseUrl = request != null
				? $"{request.Scheme}://{request.Host}"
				: "https://fallback-domain.bg";

			users = users
				.GroupBy(x => x.Email)
				.Select(g => g.First())
				.ToList();

			var emailTasks = users.Select(u =>
			{
				var favoriteTeamName = match.HomeTeamId == u.TeamId
					? match.HomeTeam.Name
					: match.AwayTeam.Name;

				var callbackUrl = $"{baseUrl}/Event/LiveEvents?team={Uri.EscapeDataString(favoriteTeamName)}";

				var opponent = favoriteTeamName == match.HomeTeam.Name
					? match.AwayTeam.Name
					: match.HomeTeam.Name;

				var subject = EmailTemplates.GetMatchNotificationSubject(favoriteTeamName, opponent);

				var homeLogo = match.HomeTeam.WikiInfo?.ImageUrl
				               ?? match.HomeTeam.LogoUrl;

				var awayLogo = match.AwayTeam.WikiInfo?.ImageUrl
				               ?? match.AwayTeam.LogoUrl;

				var formattedTime = _timezoneService.FormatForUserExact(
					match.MatchDate,
					u.TimeZone,
					"dddd, MMM d, yyyy • HH:mm"
				);

				return _emailSender.SendEmailAsync(
					u.Email,
					subject,
					EmailTemplates.MatchAddedEmail(
						logoUrl: LogoUrl,
						homeTeam: match.HomeTeam.Name,
						awayTeam: match.AwayTeam.Name,
						homeLogo: homeLogo,
						awayLogo: awayLogo,
						matchTime: formattedTime, 
						link: callbackUrl
					)
				);
			});

			await Task.WhenAll(emailTasks);

		}
	}
}
