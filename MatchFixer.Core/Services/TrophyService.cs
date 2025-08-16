using MatchFixer.Common.Enums;
using MatchFixer.Common.GeneralConstants;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.UserTrophyContext;
using MatchFixer.Core.ViewModels.Profile;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

using static MatchFixer.Common.EmailTemplates.EmailTemplates;
using static MatchFixer.Common.GeneralConstants.ProfilePictureConstants;

namespace MatchFixer.Core.Services
{
	public class TrophyService : ITrophyService
	{
		private readonly MatchFixerDbContext _dbContext;
		private readonly IEmailSender _emailSender;
		private readonly ITimezoneService _timezoneService;

		private readonly Dictionary<string, Func<UserTrophyContext, Task<bool>>> _trophyConditions;


		public TrophyService(MatchFixerDbContext dbContext, IEmailSender emailSender, ITimezoneService timezoneService)
		{
			_dbContext = dbContext;
			_emailSender = emailSender;
			_timezoneService = timezoneService;

			_trophyConditions = new Dictionary<string, Func<UserTrophyContext, Task<bool>>>
			{
				// First trophy for all users
				[TrophyNames.ShadowSyndicateRecruit] = ctx => Task.FromResult(true),

				// Milestones
				[TrophyNames.RookieRigger] = ctx => Task.FromResult(ctx.TotalBets >= 1),
				[TrophyNames.FixersDozen] = ctx => Task.FromResult(ctx.TotalBets >= 100),
				[TrophyNames.BetSyndicateBoss] = ctx => Task.FromResult(ctx.TotalWagered >= 2000),
				[TrophyNames.UltimateGrinder] = ctx => Task.FromResult(ctx.TotalBets >= 1000),

				// Time-based
				[TrophyNames.FixerAtDawn] = ctx =>
				{
					if (ctx.User == null || string.IsNullOrEmpty(ctx.User.TimeZone))
						return Task.FromResult(false);

					return Task.FromResult(ctx.UserBets.Any(b =>
					{
						var localBetTime = _timezoneService.ConvertToUserTime(b.BetTime, ctx.User.TimeZone);
						return localBetTime.TimeOfDay >= TimeSpan.FromHours(5) &&
						       localBetTime.TimeOfDay <= TimeSpan.FromHours(8);
					}));
				},

				[TrophyNames.MidnightFixer] = ctx =>
				{
					if (ctx.User == null || string.IsNullOrEmpty(ctx.User.TimeZone))
						return Task.FromResult(false);

					return Task.FromResult(ctx.UserBets.Any(b =>
					{
						var localBetTime = _timezoneService.ConvertToUserTime(b.BetTime, ctx.User.TimeZone);
						return localBetTime.TimeOfDay >= TimeSpan.Zero &&
						       localBetTime.TimeOfDay <= TimeSpan.FromHours(4);
					}));
				},

				[TrophyNames.WeekendWagerWarlord] = ctx =>
				{
					if (ctx.User == null || string.IsNullOrEmpty(ctx.User.TimeZone))
						return Task.FromResult(false);

					return Task.FromResult(ctx.UserBets.Any(b =>
					{
						var localBetTime = _timezoneService.ConvertToUserTime(b.BetTime, ctx.User.TimeZone);
						return localBetTime.DayOfWeek == DayOfWeek.Saturday ||
						       localBetTime.DayOfWeek == DayOfWeek.Sunday;
					}));
				},

				[TrophyNames.LastMinuteLeak] = async ctx =>
					await ctx.DbContext.Set<Bet>()
						.Include(b => b.MatchEvent)
						.Where(b => b.BetSlip.UserId == ctx.UserId &&
						            b.MatchEvent != null &&
						            EF.Functions.DateDiffMinute(b.BetTime, b.MatchEvent.MatchDate) <= 5)
						.AnyAsync(),

				// Special dates
				[TrophyNames.SilentBetShadyNight] = ctx => Task.FromResult(ctx.UserBets.Any(b =>
					b.BetTime.Date == new DateTime(2025, 12, 25))),

				[TrophyNames.FixmasMiracle] = ctx => Task.FromResult(ctx.UserBets.Count(b =>
					b.BetTime.Date == new DateTime(2025, 12, 25)) >= 3),

				[TrophyNames.NewYearNewFix] = ctx => Task.FromResult(ctx.UserBets.Any(b =>
					b.BetTime.Month == 1 && b.BetTime.Day == 1)),

				[TrophyNames.OneYearManyFixes] = ctx => Task.FromResult(
					ctx.User != null && ctx.User.CreatedOn.Date <= ctx.Now.Date.AddYears(-1)),

				[TrophyNames.SummerSyndicateSlam] = ctx => Task.FromResult(ctx.UserBets.Count(b =>
					b.BetTime.Month == 7) >= 10),

				// Outcome-based
				[TrophyNames.FixersHotStreak] = ctx => Task.FromResult(
					HasConsecutiveOutcomes(ctx.UserBets, BetStatus.Won, 3)),

				[TrophyNames.SyndicateSharpshooter] = ctx => Task.FromResult(
					ctx.UserBets.GroupBy(b => b.BetTime.Date)
						.Any(g => g.Count(b => b.Status == BetStatus.Won) >= 5)),

				[TrophyNames.RiggedToWin] = ctx => Task.FromResult(
					ctx.UserBets.Count(b => b.Status == BetStatus.Won) >= 20),

				[TrophyNames.FixGoneWrong] = ctx => Task.FromResult(
					HasConsecutiveOutcomes(ctx.UserBets, BetStatus.Lost, 3)),

				[TrophyNames.UnluckySyndicateSoldier] = ctx => Task.FromResult(
					ctx.UserBets.Count(b => b.Status == BetStatus.Lost) >= 10),

				[TrophyNames.BankrollObliterator] = ctx => Task.FromResult(
					ctx.UserBets.GroupBy(b => b.BetTime.Date)
						.Any(g => g.Count(b => b.Status == BetStatus.Lost) >= 5)),

				[TrophyNames.ComebackKingpin] = ctx => Task.FromResult(
					HasLossStreakThenWin(ctx.UserBets, 5)),

				[TrophyNames.RollercoasterRigger] = ctx => Task.FromResult(
					HasAlternatingOutcomes(ctx.UserBets, 4))
			};
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

		public async Task AwardTrophyIfNotAlreadyAsync(Guid userId, int trophyId, string profileUrl, string? notes = null)
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

				var trophy = await _dbContext.Trophies.FirstOrDefaultAsync(t => t.Id == trophyId);
				var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

				var logoUrl = LogoUrl;

				string htmlBody = EmailTrophyWon(
					logoUrl,
					trophy.IconUrl, 
					trophy.Name,
					profileUrl
				);

				var emailSubject = SubjectTrophyWonEmail(trophy.Name);

				await _emailSender.SendEmailAsync(
					user.Email,
					emailSubject,
					htmlBody
				);
			}
		}

		public async Task EvaluateTrophiesAsync(Guid userId, string profileUrl)
		{
			var now = DateTime.UtcNow;

			// Load all trophies and user's earned trophies
			var allTrophies = await _dbContext.Trophies.AsNoTracking().ToListAsync();
			var earnedIds = await _dbContext.UserTrophies
				.Where(ut => ut.UserId == userId)
				.Select(ut => ut.TrophyId)
				.ToListAsync();

			var toEvaluate = allTrophies.Where(t => !earnedIds.Contains(t.Id)).ToList();

			// Fetch user bets in one go
			var userBets = await _dbContext.Bets
				.Include(b => b.BetSlip)
				.Include(b => b.MatchEvent)
				.Where(b => b.BetSlip.UserId == userId)
				.ToListAsync();

			int totalBets = userBets.Count;
			decimal totalWagered = userBets.Sum(b => b.BetSlip.Amount);

			var user = await _dbContext.Users
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Id == userId);

			var context = new UserTrophyContext
			{
				UserBets = userBets,
				UserId = userId,
				TotalBets = totalBets,
				TotalWagered = totalWagered,
				User = user,
				Now = now,
				DbContext = _dbContext
			};

			foreach (var kv in _trophyConditions)
			{
				var trophyId = toEvaluate.FirstOrDefault(t => t.Name == kv.Key)?.Id;
				if (trophyId != null && await kv.Value(context))
				{
					await AwardTrophy(userId, trophyId.Value, profileUrl);
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

		private async Task AwardTrophy(Guid userId, int trophyId, string profileUrl, string? notes = null)
		{
			await AwardTrophyIfNotAlreadyAsync(userId, trophyId, profileUrl, notes);
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
