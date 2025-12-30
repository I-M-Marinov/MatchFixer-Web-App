using MatchFixer.Core.Contracts;
using MatchFixer.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Models.FootballAPI;

namespace MatchFixer.Core.Services
{
	public class UpcomingMatchService : IUpcomingMatchService
	{
		private readonly MatchFixerDbContext _dbContext;
		private readonly IFootballApiService _footballApiService;

		public UpcomingMatchService(MatchFixerDbContext dbContext, IFootballApiService footballApiService){
			_dbContext = dbContext;
			_footballApiService = footballApiService;
		}


		public async Task<List<UpcomingMatchDto>> GetUpcomingMatchesAsync(
			int leagueId,
			int take = 20)
		{

			var teams = await _dbContext.Teams
				.AsNoTracking()
				.Select(t => new
				{
					t.Id,
					t.Name
				})
				.ToListAsync();

			var teamByName = teams
				.ToDictionary(t => t.Name, t => t.Id, StringComparer.OrdinalIgnoreCase);

			// Load ONCE
			var existingFixtureIds = new HashSet<int>(
				await _dbContext.MatchEvents
					.Where(m => m.ApiFixtureId.HasValue)
					.Select(m => m.ApiFixtureId.Value)
					.ToListAsync()
			);

			var existingManualMatches = await _dbContext.MatchEvents
				.Where(m => !m.IsCancelled && !m.ApiFixtureId.HasValue)
				.Select(m => new
				{
					m.HomeTeamId,
					m.AwayTeamId,
					MatchDate = m.MatchDate.Date
				})
				.ToListAsync();

			bool IsManualDuplicate(Guid homeId, Guid awayId, DateTimeOffset kickoff)
			{
				var kickoffUtc = kickoff.UtcDateTime.Date;

				return existingManualMatches.Any(m =>
					m.HomeTeamId == homeId &&
					m.AwayTeamId == awayId &&
					m.MatchDate == kickoffUtc
				);
			}

			// Try DB-backed upcoming matches first
			var fromDb = await _dbContext.UpcomingMatchEvents
					.AsNoTracking()
					.Include(x => x.HomeTeam)
					.Include(x => x.AwayTeam)
					.Where(x =>
						x.ApiLeagueId == leagueId &&
						!x.IsImported &&
						x.MatchDateUtc > DateTime.UtcNow)
					.OrderBy(x => x.MatchDateUtc)
					.Select(x => new UpcomingMatchDto
					{
						ApiFixtureId = x.ApiFixtureId,
						KickoffUtc = new DateTimeOffset(x.MatchDateUtc, TimeSpan.Zero),

						HomeTeamId = x.HomeTeamId,
						AwayTeamId = x.AwayTeamId,

						HomeName = x.HomeTeam.Name,
						AwayName = x.AwayTeam.Name,

						HomeLogo = x.HomeTeam.LogoUrl,
						AwayLogo = x.AwayTeam.LogoUrl,

						HomeOdds = 1.11m,
						DrawOdds = 2.22m,
						AwayOdds = 3.33m
					})
					.ToListAsync();

			foreach (var m in fromDb)
			{
				if (m.HomeTeamId.HasValue && m.AwayTeamId.HasValue)
				{
					m.IsManualDuplicate =
						IsManualDuplicate(m.HomeTeamId.Value,
							m.AwayTeamId.Value,
							m.KickoffUtc);
				}
				else
				{
					m.IsManualDuplicate = false;
				}

				m.IsAlreadyImported =
					existingFixtureIds.Contains(m.ApiFixtureId)
					|| m.IsManualDuplicate == true;
			}


			if (fromDb.Any())
				return fromDb;

			// API fallback — MARK imported here
			var apiUpcoming = await _footballApiService
				.GetUpcomingFromApiAsync(leagueId, take);

			foreach (var m in apiUpcoming)
			{
				if (m.HomeTeamId.HasValue && m.AwayTeamId.HasValue)
				{
					m.IsManualDuplicate =
						IsManualDuplicate(m.HomeTeamId.Value,
							m.AwayTeamId.Value,
							m.KickoffUtc);
				}
				else
				{
					m.IsManualDuplicate = false;
				}

				m.IsAlreadyImported =
					existingFixtureIds.Contains(m.ApiFixtureId)
					|| m.IsManualDuplicate == true;
			}

			return apiUpcoming
				.OrderBy(x => x.KickoffUtc)
				.ToList();
		}

	}
}
