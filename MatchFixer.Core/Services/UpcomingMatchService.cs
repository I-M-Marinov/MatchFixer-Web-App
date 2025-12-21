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
			// DB first ----->  TAKE ALL
			var fromDb = await _dbContext.UpcomingMatchEvents
				.AsNoTracking()
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
					AwayOdds = 3.33m
				})
				.ToListAsync();

			if (fromDb.Any())
				return fromDb;

			// Fallback to API ----->  TAKE ${take} 
			var fromApi = await _footballApiService.GetUpcomingFromApiAsync(leagueId, 30);

			return fromApi
				.OrderBy(x => x.KickoffUtc)
				.Take(take)
				.ToList();
		}


	}
}
