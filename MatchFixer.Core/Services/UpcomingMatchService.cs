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
			// Load wwwwwwwwwwwwww ONCE
			var existingFixtureIds = new HashSet<int>(
				await _dbContext.MatchEvents
					.Where(m => m.ApiFixtureId.HasValue)
					.Select(m => m.ApiFixtureId.Value)
					.ToListAsync()
			);

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
					KickoffUtc = x.MatchDateUtc,

					HomeName = x.HomeTeam.Name,
					AwayName = x.AwayTeam.Name,

					HomeLogo = x.HomeTeam.LogoUrl,
					AwayLogo = x.AwayTeam.LogoUrl,

					HomeOdds = 1.11m,
					DrawOdds = 2.22m,
					AwayOdds = 3.33m,

					// already imported
					IsAlreadyImported = existingFixtureIds.Contains(x.ApiFixtureId)
				})
				.ToListAsync();

			if (fromDb.Any())
				return fromDb;

			// API fallback — MARK imported here
			var apiUpcoming = await _footballApiService
				.GetUpcomingFromApiAsync(leagueId, take);

			foreach (var m in apiUpcoming)
			{
				m.IsAlreadyImported = existingFixtureIds.Contains(m.ApiFixtureId);
			}

			return apiUpcoming
				.OrderBy(x => x.KickoffUtc)
				.ToList();
		}




	}
}
