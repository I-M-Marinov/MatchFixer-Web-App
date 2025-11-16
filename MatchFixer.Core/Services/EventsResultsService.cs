using MatchFixer.Core.Contracts;
using MatchFixer.Infrastructure;
using MatchFixer.Core.ViewModels.EventsResults;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer.Core.Services
{
	public class EventsResultsService: IEventsResultsService
	{

		private readonly MatchFixerDbContext _dbContext;

		public EventsResultsService(MatchFixerDbContext dbContext) => _dbContext = dbContext;

		public async Task<IReadOnlyList<EventsResults>> GetLatestAsync(int count = 10)
		{
			// Not cancelled + has result
			var query = _dbContext.MatchEvents
				.AsNoTracking()
				.Include(e => e.HomeTeam)
				.Include(e => e.AwayTeam)
				.Include(e => e.LiveResult)
				.Where(e => !e.IsCancelled && e.LiveResult != null);

			var items = await query
				.OrderByDescending(e => e.MatchDate)
				.Take(count)
				.Select(e => new EventsResults
				{
					MatchEventId = e.Id,
					HomeTeam = e.HomeTeam!.Name,
					AwayTeam = e.AwayTeam!.Name,
					HomeScore = e.LiveResult!.HomeScore,
					AwayScore = e.LiveResult!.AwayScore,
					HomeLogoUrl = e.HomeTeam!.LogoUrl,
					AwayLogoUrl = e.AwayTeam!.LogoUrl
				})
				.ToListAsync();

			return items;
		}
	}
}
