using MatchFixer.Common.Enums;
using MatchFixer.Common.GeneralConstants;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.Profile;
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

		public async Task<List<TrophyViewModel>> GetAllTrophiesWithUserStatusAsync(Guid userId)
		{
			var allTrophies = await _dbContext.Trophies.AsNoTracking().ToListAsync();

			var earned = await _dbContext.UserTrophies
				.Where(ut => ut.UserId == userId)
				.ToListAsync();

			var earnedDict = earned.ToDictionary(ut => ut.TrophyId, ut => ut.IsNew);

			var result = allTrophies.Select(t => new TrophyViewModel
			{
				TrophyId = t.Id,
				Name = t.Name,
				Description = t.Description,
				IconUrl = t.IconUrl,
				Level = t.Level,
				IsEarned = earnedDict.ContainsKey(t.Id),
				IsNew = earnedDict.TryGetValue(t.Id, out var isNew) && isNew
			}).ToList();

			return result;
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
					Notes = notes,
					IsNew = true
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
					// First trophy for all users ( just by registering ) 
					case TrophyNames.ShadowSyndicateRecruit:
						await AwardTrophy(userId, trophy.Id);
						break;

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

					// Outcome-Based Trophies
					case TrophyNames.FixersHotStreak:
						if (HasConsecutiveOutcomes(userBets, BetStatus.Won, 3))
							await AwardTrophy(userId, trophy.Id);
						break;

					case TrophyNames.SyndicateSharpshooter:
						if (userBets.GroupBy(b => b.BetTime.Date)
						    .Any(g => g.Count(b => b.Status == BetStatus.Won) >= 5))
							await AwardTrophy(userId, trophy.Id);
						break;

					case TrophyNames.RiggedToWin:
						if (userBets.Count(b => b.Status == BetStatus.Won) >= 20)
							await AwardTrophy(userId, trophy.Id);
						break;

					case TrophyNames.FixGoneWrong:
						if (HasConsecutiveOutcomes(userBets, BetStatus.Lost, 3))
							await AwardTrophy(userId, trophy.Id);
						break;

					case TrophyNames.UnluckySyndicateSoldier:
						if (userBets.Count(b => b.Status != BetStatus.Won) >= 10)
							await AwardTrophy(userId, trophy.Id);
						break;

					case TrophyNames.BankrollObliterator:
						if (userBets.GroupBy(b => b.BetTime.Date)
						    .Any(g => g.Count(b => b.Status != BetStatus.Won) >= 5))
							await AwardTrophy(userId, trophy.Id);
						break;

					case TrophyNames.ComebackKingpin:
						if (HasLossStreakThenWin(userBets, 5))
							await AwardTrophy(userId, trophy.Id);
						break;

					case TrophyNames.RollercoasterRigger:
						if (HasAlternatingOutcomes(userBets, 4))
							await AwardTrophy(userId, trophy.Id);
						break;
				}

			}
		}

		public async Task MarkTrophyAsSeenAsync(Guid userId, int trophyId)
		{
			var userTrophy = await _dbContext.UserTrophies
				.FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TrophyId == trophyId);

			if (userTrophy != null && userTrophy.IsNew)
			{
				userTrophy.IsNew = false;
				await _dbContext.SaveChangesAsync();
			}
		}

		private async Task AwardTrophy(Guid userId, int trophyId, string? notes = null)
		{
			await AwardTrophyIfNotAlreadyAsync(userId, trophyId, notes);
		}

		private bool HasConsecutiveOutcomes(List<Bet> bets, BetStatus targetStatus, int count)
		{
			int streak = 0;
			foreach (var bet in bets)
			{
				if (bet.Status == BetStatus.Pending || bet.Status == BetStatus.Voided)
					continue; // Skip irrelevant bets

				if (bet.Status == targetStatus)
				{
					streak++;
					if (streak >= count)
						return true;
				}
				else
				{
					streak = 0;
				}
			}
			return false;
		}

		private bool HasLossStreakThenWin(List<Bet> bets, int lossCount)
		{
			int losses = 0;

			for (int i = 0; i < bets.Count; i++)
			{
				if (bets[i].Status == BetStatus.Lost)
				{
					losses++;
					if (losses >= lossCount)
					{
						// Check next bet is Won
						if (i + 1 < bets.Count && bets[i + 1].Status == BetStatus.Won)
							return true;
					}
				}
				else if (bets[i].Status == BetStatus.Won)
				{
					losses = 0; // Reset loss streak
				}
			}
			return false;
		}

		private bool HasAlternatingOutcomes(List<Bet> bets, int alternations)
		{
			// Filter to only Won/Lost bets
			var filtered = bets.Where(b => b.Status == BetStatus.Won || b.Status == BetStatus.Lost).ToList();
			if (filtered.Count < alternations + 1)
				return false;

			int streak = 1;
			for (int i = 1; i < filtered.Count; i++)
			{
				if (filtered[i].Status != filtered[i - 1].Status)
				{
					streak++;
					if (streak >= alternations + 1)
						return true;
				}
				else
				{
					streak = 1;
				}
			}
			return false;
		}


	}

}
