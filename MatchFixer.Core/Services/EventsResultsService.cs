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

		public async Task<PagedDayEventResult<EventsResults>> GetByDayAsync(int offset = 0)
		{
			if (offset < 0)
				offset = 0;

			var tz = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");

			var days = (await _dbContext.MatchEvents
					.AsNoTracking()
					.Where(e => !e.IsCancelled && e.LiveResult != null)
					.Select(e => e.MatchDate.Value)
					.ToListAsync()) 
				.Select(d => DateOnly.FromDateTime(
					TimeZoneInfo.ConvertTimeFromUtc(d, tz)))
				.Distinct()
				.OrderByDescending(d => d)
				.ToList();

			if (days.Count == 0)
			{
				return new PagedDayEventResult<EventsResults>
				{
					DayIndex = 0,
					TotalDays = 0,
					Items = []
				};
			}

			// Clamp index 
			offset = Math.Clamp(offset, 0, days.Count - 1);
			var day = days[offset];

			var allEvents = await _dbContext.MatchEvents
				.AsNoTracking()
				.Where(e => !e.IsCancelled && e.LiveResult != null)
				.Include(e => e.HomeTeam)
				.Include(e => e.AwayTeam)
				.Include(e => e.LiveResult)
				.ToListAsync(); // 🔥 LOAD FIRST

			var items = allEvents
				.Where(e =>
					DateOnly.FromDateTime(
						TimeZoneInfo.ConvertTimeFromUtc(e.MatchDate.Value, tz)) == day)
				.OrderByDescending(e => e.MatchDate)
				.Select(e => new EventsResults
				{
					MatchEventId = e.Id,
					HomeTeam = e.HomeTeam!.Name,
					AwayTeam = e.AwayTeam!.Name,
					HomeScore = e.LiveResult!.HomeScore,
					AwayScore = e.LiveResult!.AwayScore,
					HomeLogoUrl = e.HomeTeam!.LogoUrl,
					AwayLogoUrl = e.AwayTeam!.LogoUrl,
					CompetitionName = string.IsNullOrWhiteSpace(e.CompetitionName)
						? null
						: e.CompetitionName,
					LeagueName =
						!string.IsNullOrEmpty(e.HomeTeam.LeagueName)
							? e.HomeTeam.LeagueName
							: !string.IsNullOrEmpty(e.AwayTeam.LeagueName)
								? e.AwayTeam.LeagueName
								: "Unknown"
				})
				.ToList();

			return new PagedDayEventResult<EventsResults>
			{
				Day = day,
				Items = items,
				DayIndex = offset,
				TotalDays = days.Count
			};
		}

	}
}
