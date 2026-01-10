using MatchFixer.Common.FootballLeagues;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Bets;
using MatchFixer.Core.ViewModels.LiveEvents;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure.Models.FootballAPI;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static MatchFixer.Common.DerbyLookup.DerbyLookup;
using static MatchFixer.Common.GeneralConstants.MatchEventConstants;



#nullable disable

namespace MatchFixer.Core.Services
{
	public class MatchEventService : IMatchEventService
	{
		private readonly MatchFixerDbContext _dbContext; 
		private readonly IUserContextService _userContextService;
		private readonly IBettingService _bettingService;
		private readonly IMatchEventNotifier _notifier;
		private readonly IOddsBoostService _oddsBoostService;
		private readonly IFootballApiService _footballApiService;



		public MatchEventService(
			MatchFixerDbContext dbContext, 
			 IUserContextService userContextService,
			 IBettingService bettingService,
			 IMatchEventNotifier notifier,
			 IOddsBoostService oddsBoostService,
			IFootballApiService footballApiService)
		{
			_dbContext = dbContext;
			_userContextService = userContextService;
			_bettingService = bettingService;
			_notifier = notifier;
			_oddsBoostService = oddsBoostService;
			_footballApiService = footballApiService;

		}

		public async Task<List<LiveEventViewModel>> GetLiveEventsAsync()
		{
			var now = DateTime.UtcNow;
			var user = await _userContextService.GetCurrentUserAsync();

			var events = await _dbContext.MatchEvents
				.Where(e =>
					e.LiveResult == null &&
					e.IsCancelled != true &&
					(
						e.IsPostponed ||
						e.MatchDate > now
					)
				)
				.Include(e => e.HomeTeam)
				.Include(e => e.AwayTeam)
				.OrderBy(e => e.IsPostponed)
				.ThenBy(e => e.MatchDate)
				.ToListAsync();

			var result = new List<LiveEventViewModel>();

			foreach (var e in events)
			{
				var (effHome, effDraw, effAway, boost) = await _oddsBoostService
					.GetEffectiveOddsAsync(e.Id, e.HomeOdds, e.DrawOdds, e.AwayOdds);

				result.Add(new LiveEventViewModel
				{
					Id = e.Id,
					HomeTeam = e.HomeTeam.Name,
					AwayTeam = e.AwayTeam.Name,
					KickoffTime = e.MatchDate.HasValue
						? DateTime.SpecifyKind(e.MatchDate.Value, DateTimeKind.Utc)
						: null,
					HomeWinOdds = e.HomeOdds ?? 0,
					DrawOdds = e.DrawOdds ?? 0,
					AwayWinOdds = e.AwayOdds ?? 0,
					EffectiveHomeWinOdds = effHome,
					EffectiveDrawOdds = effDraw,
					EffectiveAwayWinOdds = effAway,
					ActiveBoost = boost,
					BoostEndUtc = boost?.EndUtc,  
					HomeTeamLogoUrl = e.HomeTeam.LogoUrl,
					AwayTeamLogoUrl = e.AwayTeam.LogoUrl,
					IsDerby = e.IsDerby,
					IsPostponed = e.IsPostponed,
					UserTimeZone = user.TimeZone,
					ApiFixtureId = e.ApiFixtureId
				});
			}

			return result;
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
				.Include(e => e.OddsBoosts)
				.Where(e =>
					e.LiveResult == null &&
					e.IsCancelled != true &&
					(
						e.IsPostponed ||
						e.MatchDate > cutoff
					)
				)
				.OrderBy(e => e.IsPostponed)
				.ThenBy(e => e.MatchDate)
				.AsNoTracking()
				.ToListAsync();

			var viewModels = events.Select(e =>
			{
				var now = DateTime.UtcNow;

				// pick the currently active boost, if any
				var activeBoostEntity = e.OddsBoosts
					.Where(b => b.IsActive && b.StartUtc <= now && b.EndUtc >= now)
					.FirstOrDefault();

				return new LiveEventViewModel
				{
					Id = e.Id,
					HomeTeam = e.HomeTeam.Name,
					AwayTeam = e.AwayTeam.Name,
					KickoffTime = e.MatchDate.HasValue
						? DateTime.SpecifyKind(e.MatchDate.Value, DateTimeKind.Utc)
						: null,
					HomeWinOdds = e.HomeOdds ?? 0,
					DrawOdds = e.DrawOdds ?? 0,
					AwayWinOdds = e.AwayOdds ?? 0,

					EffectiveHomeWinOdds = activeBoostEntity != null ? (e.HomeOdds + activeBoostEntity.BoostValue) : e.HomeOdds,
					EffectiveDrawOdds = activeBoostEntity != null ? (e.DrawOdds + activeBoostEntity.BoostValue) : e.DrawOdds,
					EffectiveAwayWinOdds = activeBoostEntity != null ? (e.AwayOdds + activeBoostEntity.BoostValue) : e.AwayOdds,
					ApiFixtureId = e.ApiFixtureId,

					ActiveBoost = activeBoostEntity,
					BoostEndUtc = activeBoostEntity?.EndUtc,

					BoostActive = activeBoostEntity != null ? new BoostViewModel
					{
						Id = activeBoostEntity.Id,
						BoostValue = activeBoostEntity.BoostValue,
						StartUtc = activeBoostEntity.StartUtc,
						EndUtc = activeBoostEntity.EndUtc,
						MaxStakePerBet = activeBoostEntity.MaxStakePerBet,
						MaxUsesPerUser = activeBoostEntity.MaxUsesPerUser,
						Note = activeBoostEntity.Note
					} : null,

					HomeTeamLogoUrl = e.HomeTeam.LogoUrl,
					AwayTeamLogoUrl = e.AwayTeam.LogoUrl,
					IsDerby = e.IsDerby,
					UserTimeZone = user.TimeZone,
					IsCancelled = e.IsCancelled,
					MatchStatus = e.IsCancelled
						? "Cancelled"
						: e.IsPostponed
							? "Postponed"
							: e.MatchDate <= now
								? "Started"
								: "Live"

				};
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
		public async Task AddEventAsync(MatchEventFormModel model, int apiFixtureId)
		{
			var homeTeam = await _dbContext.Teams.FindAsync(model.HomeTeamId);
			var awayTeam = await _dbContext.Teams.FindAsync(model.AwayTeamId);

			if (homeTeam == null || awayTeam == null)
				throw new Exception(TeamDoesNotExist);

			var isDerby = IsDerby((int)homeTeam.TeamId, (int)awayTeam.TeamId);

			var matchEvent = new MatchEvent
			{
				Id = Guid.NewGuid(),
				HomeTeamId = model.HomeTeamId,
				AwayTeamId = model.AwayTeamId,
				MatchDate = model.MatchDate,
				HomeOdds = model.HomeOdds,
				DrawOdds = model.DrawOdds,
				AwayOdds = model.AwayOdds,
				IsDerby = isDerby,
				ApiFixtureId = apiFixtureId  
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

		public async Task<bool> EditMatchEventAsync(Guid matchEventId, decimal homeOdds, decimal drawOdds, decimal awayOdds, DateTime? kickoffTime)
		{
			var userId = _userContextService.GetUserId();
			var match = await _dbContext.MatchEvents.FindAsync(matchEventId);
			if (match == null || match.IsCancelled)
			{
				throw new Exception(MatchUpdateFailed);
			}

			var changes = new (string Property, object? OldValue, object? NewValue, Action Apply)[]
			{
				(nameof(match.HomeOdds), match.HomeOdds, homeOdds, () => match.HomeOdds = homeOdds),
				(nameof(match.DrawOdds), match.DrawOdds, drawOdds, () => match.DrawOdds = drawOdds),
				(nameof(match.AwayOdds), match.AwayOdds, awayOdds, () => match.AwayOdds = awayOdds),
				(nameof(match.MatchDate), match.MatchDate, kickoffTime, () => match.MatchDate = kickoffTime)
			};

			bool anyChange = false;

			foreach (var (property, oldVal, newVal, apply) in changes)
			{
				if (!Equals(oldVal, newVal))
				{
					LogChange(match.Id, property, oldVal, newVal, userId);
					apply();
					anyChange = true;
				}
			}

			if (!anyChange)
				return false;

			await _dbContext.SaveChangesAsync();
			await _notifier.NotifyMatchEventUpdatedAsync(matchEventId, homeOdds, drawOdds, awayOdds);

			return true;
		}

		public async Task<bool> CancelMatchEventAsync(Guid matchEventId)
		{
			var match = await _dbContext.MatchEvents.FindAsync(matchEventId);
			if (match == null || match.IsCancelled)
				return false;

			match.IsCancelled = true;

			var betsCancelled = await _bettingService.CancelBetsForMatchAsync(matchEventId);

			if (!betsCancelled)
				return false;

			await _dbContext.SaveChangesAsync();
			return true;
		}

		private void LogChange(Guid matchEventId, string property, object? oldValue, object? newValue, Guid changedByUserId)
		{
			if (Equals(oldValue, newValue))
				return;

			_dbContext.MatchEventLogs.Add(new MatchEventLog()
			{
				MatchEventId = matchEventId,
				PropertyName = property,
				OldValue = oldValue?.ToString(),
				NewValue = newValue?.ToString(),
				ChangedByUserId = changedByUserId,
				ChangedAt = DateTime.UtcNow
			});
		}

		public async Task<Dictionary<Guid, OddsDTO>> GetOddsForMatchesAsync(Guid[] matchIds)
		{
			var now = DateTime.UtcNow;

			return await _dbContext.MatchEvents
				.Where(me => matchIds.Contains(me.Id))
				.Select(me => new OddsDTO
				{
					Id = me.Id,
					HomeOdds = me.HomeOdds,
					DrawOdds = me.DrawOdds,
					AwayOdds = me.AwayOdds,

					// Look up any active boost for this match
					HasActiveBoost = me.OddsBoosts.Any(b => b.IsActive && b.StartUtc <= now && b.EndUtc >= now),
					EffectiveHomeOdds = me.HomeOdds + me.OddsBoosts
						.Where(b => b.IsActive && b.StartUtc <= now && b.EndUtc >= now)
						.OrderByDescending(b => b.BoostValue)
						.Select(b => b.BoostValue)
						.FirstOrDefault(),
					EffectiveDrawOdds = me.DrawOdds + me.OddsBoosts
						.Where(b => b.IsActive && b.StartUtc <= now && b.EndUtc >= now)
						.OrderByDescending(b => b.BoostValue)
						.Select(b => b.BoostValue)
						.FirstOrDefault(),
					EffectiveAwayOdds = me.AwayOdds + me.OddsBoosts
						.Where(b => b.IsActive && b.StartUtc <= now && b.EndUtc >= now)
						.OrderByDescending(b => b.BoostValue)
						.Select(b => b.BoostValue)
						.FirstOrDefault()
				})
				.ToDictionaryAsync(x => x.Id, x => x);
		}

		public Task<List<ApiLeagueSelectViewModel>> GetApiLeaguesAsync()
		{
			var leagues = SupportedApiLeagues.Football
				.Select(l => new ApiLeagueSelectViewModel
				{
					ApiLeagueId = l.Key,
					Name = l.Value
				})
				.ToList();

			return Task.FromResult(leagues);
		}

		public async Task<bool> MatchExistsByApiFixtureAsync(int apiFixtureId)
		{
			return await _dbContext.MatchEvents
				.AnyAsync(e => e.ApiFixtureId == apiFixtureId);
		}

		public async Task AddEventFromUpcomingAsync(UpcomingMatchDto m)
		{
			if (await MatchExistsByApiFixtureAsync(m.ApiFixtureId))
				return;

			var homeId = await ResolveTeamIdAsync(m.HomeName);
			var awayId = await ResolveTeamIdAsync(m.AwayName);

			var model = new MatchEventFormModel
			{
				HomeTeamId = homeId,
				AwayTeamId = awayId,
				MatchDate = m.KickoffUtc.UtcDateTime,
				HomeOdds = m.HomeOdds,
				DrawOdds = m.DrawOdds,
				AwayOdds = m.AwayOdds
			};

			await AddEventAsync(model, m.ApiFixtureId);
		}

		private async Task<Guid> ResolveTeamIdAsync(string teamName, string? logoUrl = null)
		{
			if (!string.IsNullOrEmpty(logoUrl))
			{
				var apiTeamId = _footballApiService.ExtractTeamIdFromLogoUrl(logoUrl);

				if (apiTeamId.HasValue)
				{
					var byApiId = await _dbContext.Teams
						.Where(t => t.TeamId == apiTeamId.Value)
						.Select(t => t.Id)
						.FirstOrDefaultAsync();

					if (byApiId != Guid.Empty)
						return byApiId;
				}
			}

			var byName = await _dbContext.Teams
				.Where(t => t.Name == teamName)
				.Select(t => t.Id)
				.FirstOrDefaultAsync();

			if (byName != Guid.Empty)
				return byName;

			throw new InvalidOperationException(
				$"Team '{teamName}' not found in database. Seed teams first."
			);
		}

		public async Task<bool> PostponeMatchEventAsync(Guid matchEventId, Guid userId)
		{
			var match = await _dbContext.MatchEvents
				.FirstOrDefaultAsync(m => m.Id == matchEventId);

			if (match == null)
				throw new InvalidOperationException("Match not found.");

			match.IsPostponed = true;
			match.MatchDate = null;

			// Hard stop any boosts (clean state)
			var activeBoosts = await _dbContext.OddsBoosts
				.Where(b => b.MatchEventId == matchEventId && b.IsActive)
				.ToListAsync();

			foreach (var boost in activeBoosts)
			{
				boost.IsActive = false;
				boost.EndUtc = DateTime.UtcNow;
			}

			await _dbContext.SaveChangesAsync();
			return true;
		}


	}
}
