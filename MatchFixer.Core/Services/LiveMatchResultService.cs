using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.MatchResults;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer.Core.Services
{
	public class LiveMatchResultService : ILiveMatchResultService
	{
		private readonly MatchFixerDbContext _dbContext;
		private readonly IBettingService _bettingService;
		private readonly ITimezoneService _timezoneService;
		private readonly ISessionService _sessionService;

		public LiveMatchResultService(
			MatchFixerDbContext dbContext, 
			ITimezoneService timezoneService,
			ISessionService sessionService,
			IBettingService bettingService)
		{
			_dbContext = dbContext;
			_timezoneService = timezoneService;
			_sessionService = sessionService;
			_bettingService = bettingService;
		}

		public async Task<bool> AddMatchResultAsync(
			Guid matchEventId,
			int homeScore,
			int awayScore,
			string? notes = null)
		{
			var match = await _dbContext.MatchEvents
				.Include(m => m.LiveResult)
				.FirstOrDefaultAsync(m => m.Id == matchEventId);

			if (match == null)
				return false; // match event was not found 

			if (match.Status != MatchStatus.FullTime)
				return false; // Only FullTime matches can be scored

			if (match.LiveResult != null)
				return false; // already has a result recorded

			var result = new LiveMatchResult
			{
				MatchEventId = match.Id,
				HomeScore = homeScore,
				AwayScore = awayScore,
				Notes = notes,
				RecordedAt = DateTime.UtcNow
			};

			_dbContext.LiveMatchResults.Add(result);
			match.LiveResult = result;

			await _dbContext.SaveChangesAsync();

			var relatedSlips = await _dbContext.BetSlips
				.Where(bs => bs.Bets.Any(b => b.MatchEventId == match.Id))
				.Select(bs => bs.Id)
				.ToListAsync();

			foreach (var slipId in relatedSlips)
			{
				await _bettingService.EvaluateBetSlipAsync(slipId);
			}

			return true;
		}



		public async Task<List<MatchResultInputViewModel>> GetUnresolvedMatchResultsAsync()
		{
			var timeZoneId = _sessionService.GetUserTimezone();

			var matches = await _dbContext.MatchEvents
				.Include(m => m.HomeTeam)
				.Include(m => m.AwayTeam)
				.Include(m => m.LiveResult)
				.Where(m => m.LiveResult == null)
				.Where(m => m.Status == MatchStatus.FullTime 
				)
				.OrderBy(m => m.MatchDate)
				.ToListAsync();

			return matches.Select(m => new MatchResultInputViewModel
			{
				MatchId = m.Id,
				HomeTeam = m.HomeTeam.Name,
				AwayTeam = m.AwayTeam.Name,
				MatchDate = m.MatchDate,
				DisplayTime = _timezoneService.FormatForUserBets(m.MatchDate, timeZoneId)
			}).ToList();
		}

		public async Task<bool> MarkMatchAsFullTimeAsync(Guid matchId)
		{
			var match = await _dbContext.MatchEvents.FindAsync(matchId);

			if (match == null)
				return false;

			if (match.Status == MatchStatus.Cancelled ||
			    match.Status == MatchStatus.Postponed ||
			    match.Status == MatchStatus.FullTime)
				return false;

			if (match.MatchDate.HasValue &&
			    match.MatchDate > DateTime.UtcNow)
				return false;

			match.Status = MatchStatus.FullTime;
			match.FinishedAtUtc = DateTime.UtcNow;

			await _dbContext.SaveChangesAsync();

			return true;
		}
	}
}
