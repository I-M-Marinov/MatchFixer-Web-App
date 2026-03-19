using MatchFixer.Common.Enums;
using MatchFixer.Infrastructure;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.ViewModels;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Dashboard;
using MatchFixer_Web_App.Areas.Admin.ViewModels.MatchEvents;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer_Web_App.Areas.Admin.Services
{

	namespace MatchFixer_Web_App.Areas.Admin.Services
	{
		public class AdminDashboardService : IAdminDashboardService
		{
			private readonly MatchFixerDbContext _dbContext;

			public AdminDashboardService(MatchFixerDbContext dbContext)
			{
				_dbContext = dbContext;
			}

			public async Task<AdminDashboardViewModel> GetDashboardAsync()
			{
				var now = DateTime.UtcNow;
				var todayStart = now.Date;
				var yesterdayStart = todayStart.AddDays(-1);
				var weekStart = todayStart.AddDays(-7);
				var last7 = DateTime.UtcNow.AddDays(-7);

				/* -----------------------------
				   USERS
				----------------------------- */

				var totalUsers = await _dbContext.Users.CountAsync();
				var activeUsers = await _dbContext.Users.CountAsync(u => u.EmailConfirmed && !u.IsDeleted);
				var deletedUsers = await _dbContext.Users.CountAsync(u => u.IsDeleted);
				var lockedUsers = await _dbContext.Users.CountAsync(u => u.LockoutEnd > now);
				var bannedUsers = await _dbContext.Users.CountAsync(u => !u.IsActive && u.WasDeactivatedByAdmin);

				/* -----------------------------
				   WALLET
				----------------------------- */

				var totalWalletBalance = await _dbContext.Wallets.SumAsync(w => (decimal?)w.Balance) ?? 0m;
				var totalTransactions = await _dbContext.WalletTransactions.CountAsync();

				/* -----------------------------
				   BETS
				----------------------------- */

				var totalBets = await _dbContext.Bets.CountAsync();
				var pendingBets = await _dbContext.Bets.CountAsync(b => b.Status == BetStatus.Pending);
				var wonBets = await _dbContext.Bets.CountAsync(b => b.Status == BetStatus.Won);
				var lostBets = await _dbContext.Bets.CountAsync(b => b.Status == BetStatus.Lost);

				/* -----------------------------
				   PLATFORM EXPOSURE
				----------------------------- */

				var slips = await _dbContext.BetSlips
					.Where(s =>
						!s.IsSettled &&
						s.Bets.Any(b =>
							b.MatchEvent.MatchDate >= todayStart &&
							b.MatchEvent.MatchDate < todayStart.AddDays(1)))
					.Select(s => new
					{
						s.Amount,
						Odds = s.Bets.Select(b => b.Odds)
					})
					.ToListAsync();

				var stakesToday = await _dbContext.BetSlips
					.Where(s => s.Bets.Any(b =>
						b.MatchEvent.MatchDate >= todayStart &&
						b.MatchEvent.MatchDate < todayStart.AddDays(1)))
					.SumAsync(s => (decimal?)s.Amount) ?? 0m;
			
				var potentialPayout = slips.Sum(s =>
				{
					var totalOdds = s.Odds.Aggregate(1m, (acc, o) => acc * o);
					return s.Amount * totalOdds;
				});

				var exposureVm = new PlatformExposureViewModel
				{
					TotalStakesToday = stakesToday,
					PotentialPayout = potentialPayout
				};

				/* -----------------------------
				   BETS ACTIVITY
				----------------------------- */

				var betsToday = await _dbContext.Bets
					.CountAsync(b => b.BetTime >= todayStart);

				var betsYesterday = await _dbContext.Bets
					.CountAsync(b => b.BetTime >= yesterdayStart && b.BetTime < todayStart);

				var betsThisWeek = await _dbContext.Bets
					.CountAsync(b => b.BetTime >= weekStart);

				var betsActivity = new BetsActivityViewModel
				{
					BetsToday = betsToday,
					BetsYesterday = betsYesterday,
					BetsThisWeek = betsThisWeek
				};

				/* -----------------------------
				   WALLET FLOW
				----------------------------- */

				var depositsToday = await _dbContext.WalletTransactions
					.Where(t => t.TransactionType == WalletTransactionType.Deposit && t.CreatedAt >= todayStart)
					.SumAsync(t => (decimal?)t.Amount) ?? 0m;

				var withdrawalsToday = await _dbContext.WalletTransactions
					.Where(t => t.TransactionType == WalletTransactionType.Withdrawal && t.CreatedAt >= todayStart)
					.SumAsync(t => (decimal?)t.Amount) ?? 0m;

				var walletActivity = new WalletActivityViewModel
				{
					DepositsToday = depositsToday,
					WithdrawalsToday = withdrawalsToday
				};

				/* -----------------------------
				   TOP WINNERS (48h)
				----------------------------- */

				var twoDaysAgo = now.AddDays(-2);

				var topWinners = await _dbContext.BetSlips
					.Where(s => s.BetTime >= twoDaysAgo)
					.Where(s => s.Bets.All(b => b.Status == BetStatus.Won))
					.GroupBy(s => new
					{
						s.UserId,
						s.User.FirstName,
						s.User.LastName,
						s.User.Email
					})
					.Select(g => new TopUserWinLossRow
					{
						UserId = g.Key.UserId,
						Username = g.Key.FirstName + " " + g.Key.LastName,
						Email = g.Key.Email,
						Profit = g.Sum(s => (s.WinAmount ?? 0) - s.Amount)
					})
					.OrderByDescending(x => x.Profit)
					.Take(5)
					.ToListAsync();

				/* -----------------------------
				   TOP LOSERS (48h)
				----------------------------- */

				var topLosers = await _dbContext.BetSlips
					.Where(s => s.BetTime >= twoDaysAgo)
					.Where(s => s.Bets.Any(b => b.Status == BetStatus.Lost))
					.GroupBy(s => new
					{
						s.UserId,
						s.User.FirstName,
						s.User.LastName,
						s.User.Email
					})
					.Select(g => new TopUserWinLossRow
					{
						UserId = g.Key.UserId,
						Username = g.Key.FirstName + " " + g.Key.LastName,
						Email = g.Key.Email,
						Profit = g.Sum(s => s.Amount)
					})
					.OrderByDescending(x => x.Profit)
					.Take(5)
					.ToListAsync();

				/* -----------------------------
				   HOT MATCHES
				----------------------------- */

				var hotMatches = await _dbContext.Bets
					.Where(b => b.MatchEvent.MatchDate >= last7)
					.GroupBy(b => b.MatchEventId)
					.Select(g => new
					{
						MatchId = g.Key,
						BetsCount = g.Count()
					})
					.OrderByDescending(x => x.BetsCount)
					.Take(5)
					.Join(_dbContext.MatchEvents,
						g => g.MatchId,
						m => m.Id,
						(g, m) => new HotMatchRow
						{
							MatchId = m.Id,
							HomeTeam = m.HomeTeam.Name,
							AwayTeam = m.AwayTeam.Name,
							HomeLogo = m.HomeTeam.LogoUrl,
							AwayLogo = m.AwayTeam.LogoUrl,
							HomeTeamLocalLogoUrl = m.HomeTeam.LocalLogoUrl,
							AwayTeamLocalLogoUrl = m.AwayTeam.LocalLogoUrl,
							BetsCount = g.BetsCount
						})
					.ToListAsync();

				/* -----------------------------
				   AVERAGE BET SIZES
				----------------------------- */

				var avgBetToday = await _dbContext.BetSlips
					.AsNoTracking()
					.Where(s => s.BetTime >= todayStart)
					.AverageAsync(s => (decimal?)s.Amount) ?? 0;

				var avgBetYesterday = await _dbContext.BetSlips
					.AsNoTracking()
					.Where(s => s.BetTime >= yesterdayStart && s.BetTime < todayStart)
					.AverageAsync(s => (decimal?)s.Amount) ?? 0;

				var avgBetWeek = await _dbContext.BetSlips
					.AsNoTracking()
					.Where(s => s.BetTime >= weekStart)
					.AverageAsync(s => (decimal?)s.Amount) ?? 0;


				return new AdminDashboardViewModel
				{
					TotalUsers = totalUsers,
					ActiveUsers = activeUsers,
					DeletedUsers = deletedUsers,
					LockedUsers = lockedUsers,
					BannedUsers = bannedUsers,

					TotalWalletBalance = totalWalletBalance,
					TotalTransactions = totalTransactions,

					TotalBets = totalBets,
					PendingBets = pendingBets,
					WonBets = wonBets,
					LostBets = lostBets,

					Exposure = exposureVm,
					BetsActivity = betsActivity,
					WalletActivity = walletActivity,

					TopWinners = topWinners,
					TopLosers = topLosers,
					HotMatches = hotMatches,
					AverageBetSizes = new List<AverageBetSizeViewModel>
					{
						new()
						{
							Period = AverageBetSizeCycle.Today,
							AverageAmount = avgBetToday
						},
						new()
						{
							Period = AverageBetSizeCycle.Yesterday,
							AverageAmount = avgBetYesterday
						},
						new()
						{
							Period = AverageBetSizeCycle.ThisWeek,
							AverageAmount = avgBetWeek
						}
					}
				};
			}

			public async Task<AdminEventsSummaryViewModel> BuildEventsSummaryAsync(int takeRecent = 10)
			{
				var now = DateTime.UtcNow;
				var todayStart = now.Date;
				var sevenDaysAgo = todayStart.AddDays(-6); // last 7 calendar days inclusive of today

				// Base query
				var q = _dbContext.MatchEvents
					.AsNoTracking()
					.Include(m => m.HomeTeam)
					.Include(m => m.AwayTeam)
					.Include(m => m.LiveResult);

				var total = await q.CountAsync();
				var finished = await q.CountAsync(m => m.LiveResult != null);
				var cancelled = await q.CountAsync(m => m.IsCancelled);

				// Live: started, not finished, not cancelled
				var live = await q.CountAsync(m =>
					m.MatchDate <= now &&
					m.LiveResult == null &&
					!m.IsCancelled);

				// Upcoming
				var upcoming = await q.CountAsync(m =>
					m.MatchDate > now &&
					!m.IsCancelled);

				var today = await q.CountAsync(m =>
					m.MatchDate >= todayStart &&
					m.MatchDate < todayStart.AddDays(1));

				var last7 = await q.CountAsync(m =>
					m.MatchDate >= sevenDaysAgo &&
					m.MatchDate < todayStart.AddDays(1));

				// Events with any voided bets
				var eventsWithVoidedBets = await _dbContext.Bets
					.Where(b => b.Status == BetStatus.Voided)
					.Select(b => b.MatchEventId)
					.Distinct()
					.CountAsync();

				// Recently updated
				var recent = await q
					.OrderByDescending(m => m.MatchDate)
					.Take(takeRecent)
					.Select(m => new AdminEventRow
					{
						Id = m.Id,
						League = null, 
						HomeTeam = m.HomeTeam.Name,
						AwayTeam = m.AwayTeam.Name,
						HomeTeamLogo = m.HomeTeam.LogoUrl,
						AwayTeamLogo = m.AwayTeam.LogoUrl,
						MatchUtc = m.MatchDate,
						Status = m.IsCancelled
										? nameof(MatchStatus.Cancelled)
										: (m.LiveResult != null ? $"{m.LiveResult.HomeScore} : {m.LiveResult.AwayScore}" : (m.MatchDate <= now ? nameof(MatchStatus.Live) : nameof(MatchStatus.Scheduled))),
						UpdatedUtc = null
					})
					.ToListAsync();

				return new AdminEventsSummaryViewModel
				{
					TotalEvents = total,
					Upcoming = upcoming,
					Live = live,
					Finished = finished,
					Canceled = cancelled,
					Postponed = 0,
					EventsWithVoidedBets = eventsWithVoidedBets,
					Today = today,
					Last7Days = last7,
					RecentUpdated = recent
				};
			}
		}
	}
}


