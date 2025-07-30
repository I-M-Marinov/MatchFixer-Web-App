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
		private readonly IWalletService _walletService;
		private readonly ITimezoneService _timezoneService;
		private readonly ISessionService _sessionService;

		public LiveMatchResultService(
			MatchFixerDbContext dbContext, 
			IWalletService walletService,
			ITimezoneService timezoneService,
			ISessionService sessionService)
		{
			_dbContext = dbContext;
			_walletService = walletService;
			_timezoneService = timezoneService;
			_sessionService = sessionService;
		}

		public async Task<bool> AddMatchResultAsync(Guid matchEventId, int homeScore, int awayScore, string? notes = null)
		{
			var match = await _dbContext.MatchEvents
				.Include(m => m.LiveResult)
				.Include(m => m.HomeTeam)
				.Include(m => m.AwayTeam)
				.FirstOrDefaultAsync(m => m.Id == matchEventId);

			if (match == null || match.LiveResult != null)
				return false;

			// Save result
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

			var winningPick = homeScore > awayScore
				? MatchPick.Home
				: homeScore < awayScore
					? MatchPick.Away
					: MatchPick.Draw;

			// Get all bet slips that include a bet for this match and are not yet settled
			var betSlips = await _dbContext.BetSlips
				.Include(bs => bs.Bets)
				.Where(bs => !bs.IsSettled && bs.Bets.Any(b => b.MatchEventId == match.Id))
				.ToListAsync();


			foreach (var slip in betSlips)
			{
				bool allResolved = true;
				bool allWinning = true;
				decimal totalOdds = 1.0m;

				var activeBets = slip.Bets.Where(b => b.Status != BetStatus.Voided).ToList();

				foreach (var bet in activeBets)
				{
					var betMatch = await _dbContext.MatchEvents
						.Include(m => m.LiveResult)
						.FirstOrDefaultAsync(m => m.Id == bet.MatchEventId);

					if (betMatch?.LiveResult == null)
					{
						allResolved = false;
						break;
					}

					var actualOutcome = betMatch.LiveResult.HomeScore > betMatch.LiveResult.AwayScore
						? MatchPick.Home
						: betMatch.LiveResult.HomeScore < betMatch.LiveResult.AwayScore
							? MatchPick.Away
							: MatchPick.Draw;

					if (bet.Pick == actualOutcome)
					{
						bet.Status = BetStatus.Won;
						totalOdds *= bet.Odds;
					}
					else
					{
						bet.Status = BetStatus.Lost;
						allWinning = false;
					}
				}

				if (allWinning && allResolved)
				{
					slip.IsSettled = true;
					var winnings = slip.Amount * totalOdds;
					slip.WinAmount = winnings;

					await _walletService.AwardWinningsAsync(
						userId: slip.UserId,
						amount: winnings,
						matchDescription: $"Winnings for slip # {slip.Id}, submitted on {slip.BetTime}"
					);
				}
				else if (!allWinning)
				{
					// At least one bet lost – settle as a losing slip immediately
					slip.IsSettled = true;
					slip.WinAmount = 0;
				}
			}

			await _dbContext.SaveChangesAsync();
			return true;
		}

		public async Task<List<MatchResultInputViewModel>> GetUnresolvedMatchResultsAsync()
		{
			var timeZoneId = _sessionService.GetUserTimezone();

			var cutoffTime = DateTime.UtcNow.AddHours(-2.5);

			var matches = await _dbContext.MatchEvents
				.Include(m => m.HomeTeam)
				.Include(m => m.AwayTeam)
				.Include(m => m.LiveResult)
				.Where(m => m.LiveResult == null)
				.Where(m => m.MatchDate <= cutoffTime) // Only matches that started more than two and a half hours ago 
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
	}
}
