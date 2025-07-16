using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.LiveEvents;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;

using static MatchFixer.Common.DerbyLookup.DerbyLookup;
using static MatchFixer.Common.GeneralConstants.MatchEventConstants;


#nullable disable

namespace MatchFixer.Core.Services
{
	public class MatchEventService : IMatchEventService
	{
		private readonly MatchFixerDbContext _dbContext; 
		private readonly IUserContextService _userContextService;


		public MatchEventService(
			MatchFixerDbContext dbContext, 
			 IUserContextService userContextService)
		{
			_dbContext = dbContext;
			_userContextService = userContextService;
		}

		public async Task<List<LiveEventViewModel>> GetLiveEventsAsync()
		{
			var now = DateTime.UtcNow;

			var user = await _userContextService.GetCurrentUserAsync();

			var events = await _dbContext.MatchEvents
				.Where(e => e.MatchDate > now && e.LiveResult == null && e.IsCancelled != true) // Only upcoming matches + result submitted yet + matches that are not already voided
				.Include(e => e.HomeTeam)
				.Include(e => e.AwayTeam)
				.OrderBy(e => e.MatchDate)
				.Select(e => new LiveEventViewModel
				{
					Id = e.Id,
					HomeTeam = e.HomeTeam.Name,
					AwayTeam = e.AwayTeam.Name,
					KickoffTime = DateTime.SpecifyKind(e.MatchDate, DateTimeKind.Utc),
					HomeWinOdds = e.HomeOdds ?? 0,
					DrawOdds = e.DrawOdds ?? 0,
					AwayWinOdds = e.AwayOdds ?? 0,
					HomeTeamLogoUrl = e.HomeTeam.LogoUrl,
					AwayTeamLogoUrl = e.AwayTeam.LogoUrl,
					IsDerby = e.IsDerby,
					UserTimeZone = user.TimeZone
				})
				.AsNoTracking()
				.ToListAsync();

			return events;
		}

		public async Task<List<LiveEventViewModel>> GetAllEventsAsync()
		{
			var now = DateTime.UtcNow;
			var user = await _userContextService.GetCurrentUserAsync();
			var cutoff = now.AddHours(-2.5);

			var events = await _dbContext.MatchEvents
				.Include(e => e.HomeTeam)
				.Include(e => e.AwayTeam)
				.Include(e => e.LiveResult)
				.Where(e => e.LiveResult == null && e.IsCancelled != true && e.MatchDate > cutoff) // Only upcoming matches + result submitted yet + matches that are not already voided
				.OrderBy(e => e.MatchDate)
				.AsNoTracking()
				.ToListAsync();

			var viewModels = events.Select(e => new LiveEventViewModel
			{
				Id = e.Id,
				HomeTeam = e.HomeTeam.Name,
				AwayTeam = e.AwayTeam.Name,
				KickoffTime = DateTime.SpecifyKind(e.MatchDate, DateTimeKind.Utc),
				HomeWinOdds = e.HomeOdds ?? 0,
				DrawOdds = e.DrawOdds ?? 0,
				AwayWinOdds = e.AwayOdds ?? 0,
				HomeTeamLogoUrl = e.HomeTeam.LogoUrl,
				AwayTeamLogoUrl = e.AwayTeam.LogoUrl,
				IsDerby = e.IsDerby,
				IsCancelled = e.IsCancelled,
				UserTimeZone = user.TimeZone,
				MatchStatus = e.IsCancelled
					? "Cancelled"
					: e.MatchDate <= now
						? "Started"
						: "Live"
			}).ToList();

			return viewModels;
		}



		public async Task AddEventAsync(MatchEventFormModel model)
		{
			var homeTeam = await _dbContext.Teams.FindAsync(model.HomeTeamId);
			var awayTeam = await _dbContext.Teams.FindAsync(model.AwayTeamId);

			if (homeTeam == null || awayTeam == null)
			{
				throw new Exception(TeamDoesNotExist);
			}

			var utcMatchDate = DateTime.SpecifyKind(model.MatchDate, DateTimeKind.Utc);
			var isDerby = IsDerby((int)homeTeam.TeamId, (int)awayTeam.TeamId);


			var matchEvent = new MatchEvent
			{
				Id = Guid.NewGuid(),
				HomeTeamId = model.HomeTeamId,
				AwayTeamId = model.AwayTeamId,
				MatchDate = utcMatchDate,
				HomeOdds = model.HomeOdds,
				DrawOdds = model.DrawOdds,
				AwayOdds = model.AwayOdds,
				IsDerby = isDerby
			};

			await _dbContext.MatchEvents.AddAsync(matchEvent);
			await _dbContext.SaveChangesAsync();
		}

		public async Task<string> GetTeamLogo(string name)
		{
			var logo = await _dbContext.Teams
				.Where(t => t.Name == name)
				.Select(t => t.LogoUrl.ToString())
				.AsNoTracking()
				.FirstOrDefaultAsync();

			return logo;
		}

		public async Task<Dictionary<string, List<SelectListItem>>> GetTeamsGroupedByLeagueAsync()
		{
			var teams = await _dbContext.Teams
				.OrderBy(t => t.LeagueName)
				.ThenBy(t => t.Name)
				.AsNoTracking()
				.ToListAsync();

			var teamsByLeague = teams
				.GroupBy(t => t.LeagueName)
				.ToDictionary(
					group => group.Key,
					group => group.Select(t => new SelectListItem
					{
						Value = t.Id.ToString(),
						Text = $"{t.Name}|{t.LogoUrl}"
					}).ToList()
				);

			return teamsByLeague;
		}

		public async Task<bool> EditMatchEventAsync(Guid matchEventId, decimal homeOdds, decimal drawOdds, decimal awayOdds, DateTime kickoffTime)
		{
			var match = await _dbContext.MatchEvents.FindAsync(matchEventId);
			if (match == null || match.IsCancelled)
				return false;

			match.HomeOdds = homeOdds;
			match.DrawOdds = drawOdds;
			match.AwayOdds = awayOdds;
			match.MatchDate = kickoffTime;

			await _dbContext.SaveChangesAsync();
			return true;
		}

		public async Task<bool> CancelMatchEventAsync(Guid matchEventId)
		{
			var match = await _dbContext.MatchEvents.FindAsync(matchEventId);
			if (match == null || match.IsCancelled)
				return false;

			match.IsCancelled = true;
			await _dbContext.SaveChangesAsync();
			return true;
		}


	}
}
