using MatchFixer.Common.GeneralConstants;
using MatchFixer.Core.Contracts;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer.Core.Services
{
	public class TrophyService : ITrophyService
	{
		private readonly MatchFixerDbContext _dbContext;

		public TrophyService(MatchFixerDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task AwardTrophyIfNotAlreadyAsync(Guid userId, int trophyId, string? notes = null)
		{
			bool alreadyAwarded = await _dbContext.UserTrophies
				.AnyAsync(ut => ut.UserId == userId && ut.TrophyId == trophyId);

			if (!alreadyAwarded)
			{
				var userTrophy = new UserTrophy
				{
					UserId = userId,
					TrophyId = trophyId,
					AwardedOn = DateTime.UtcNow,
					Notes = notes
				};

				_dbContext.UserTrophies.Add(userTrophy);
				await _dbContext.SaveChangesAsync();
			}
		}

		public async Task EvaluateTrophiesAsync(Guid userId)
		{
			var now = DateTime.UtcNow;

			// Load all trophies and user's earned trophies
			var allTrophies = await _dbContext.Trophies.AsNoTracking().ToListAsync();

			var earnedIds = await _dbContext.UserTrophies
				.Where(ut => ut.UserId == userId)
				.Select(ut => ut.TrophyId)
				.ToListAsync();

			var toEvaluate = allTrophies.Where(t => !earnedIds.Contains(t.Id)).ToList();

			// Fetch user bets + related BetSlip + MatchEvent in a single query
			var userBets = await _dbContext.Bets
				.Include(b => b.BetSlip)
				.Include(b => b.MatchEvent)
				.Where(b => b.BetSlip.UserId == userId)
				.ToListAsync();

			int totalBets = userBets.Count;
			decimal totalWagered = userBets.Sum(b => b.BetSlip.Amount);

			foreach (var trophy in toEvaluate)
			{
				switch (trophy.Name)
				{
					// Milestone Trophies
					case TrophyNames.RookieRigger:
						if (totalBets >= 1)
							await AwardTrophy(userId, trophy.Id);
						break;

					case TrophyNames.FixersDozen:
						if (totalBets >= 100)
							await AwardTrophy(userId, trophy.Id);
						break;

					case TrophyNames.BetSyndicateBoss:
						if (totalWagered >= 2000)
							await AwardTrophy(userId, trophy.Id);
						break;

					case TrophyNames.UltimateGrinder:
						if (totalBets >= 1000)
							await AwardTrophy(userId, trophy.Id);
						break;

					// Time-Based Trophies
					case TrophyNames.FixerAtDawn:
						if (userBets.Any(b => b.BetTime.TimeOfDay >= TimeSpan.FromHours(5) &&
											  b.BetTime.TimeOfDay <= TimeSpan.FromHours(8)))
							await AwardTrophy(userId, trophy.Id);
						break;

					case TrophyNames.MidnightFixer:
						if (userBets.Any(b => b.BetTime.TimeOfDay >= TimeSpan.Zero &&
											  b.BetTime.TimeOfDay <= TimeSpan.FromHours(4)))
							await AwardTrophy(userId, trophy.Id);
						break;

					case TrophyNames.WeekendWagerWarlord:
						if (userBets.Any(b => b.BetTime.DayOfWeek == DayOfWeek.Saturday ||
											  b.BetTime.DayOfWeek == DayOfWeek.Sunday))
							await AwardTrophy(userId, trophy.Id);
						break;

					case TrophyNames.LastMinuteLeak:
						bool hasLastMinuteLeak = await _dbContext.Bets
							.Include(b => b.MatchEvent)
							.Where(b => b.BetSlip.UserId == userId &&
										b.MatchEvent != null &&
										EF.Functions.DateDiffMinute(b.BetTime, b.MatchEvent.MatchDate) <= 5)
							.AnyAsync();

						if (hasLastMinuteLeak)
							await AwardTrophy(userId, trophy.Id);
						break;

					// Special Event Trophies
					case TrophyNames.SilentBetShadyNight:
						if (userBets.Any(b => b.BetTime.Date == new DateTime(2025, 12, 25)))
							await AwardTrophy(userId, trophy.Id);
						break;

					case TrophyNames.FixmasMiracle:
						if (userBets.Count(b => b.BetTime.Date == new DateTime(2025, 12, 25)) >= 3)
							await AwardTrophy(userId, trophy.Id);
						break;

					case TrophyNames.NewYearNewFix:
						if (userBets.Any(b => b.BetTime.Month == 1 && b.BetTime.Day == 1))
							await AwardTrophy(userId, trophy.Id);
						break;

					case TrophyNames.OneYearManyFixes:
						var user = await _dbContext.Users
							.AsNoTracking()
							.FirstOrDefaultAsync(u => u.Id == userId);

						if (user != null && user.CreatedOn.Date <= now.Date.AddYears(-1))
							await AwardTrophy(userId, trophy.Id);
						break;

					case TrophyNames.SummerSyndicateSlam:
						if (userBets.Count(b => b.BetTime.Month == 7) >= 10)
							await AwardTrophy(userId, trophy.Id);
						break;
				}

			}
		}


		private async Task AwardTrophy(Guid userId, int trophyId, string? notes = null)
		{
			await AwardTrophyIfNotAlreadyAsync(userId, trophyId, notes);
		}

	}

}
