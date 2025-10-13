using MatchFixer.Common.Enums;
using MatchFixer.Infrastructure;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.ViewModels;
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

				var totalUsers = await _dbContext.Users.CountAsync();
				var activeUsers = await _dbContext.Users.CountAsync(u => u.EmailConfirmed && !u.IsDeleted);
				var deletedUsers = await _dbContext.Users.CountAsync(u => u.IsDeleted);
				var lockedUsers = await _dbContext.Users.CountAsync(u => u.LockoutEnd > now);
				var bannedUsers = await _dbContext.Users.CountAsync(u => !u.IsActive && u.WasDeactivatedByAdmin);

				var totalWalletBalance = await _dbContext.Wallets.SumAsync(w => (decimal?)w.Balance) ?? 0m;
				var totalTransactions = await _dbContext.WalletTransactions.CountAsync();

				var totalBets = await _dbContext.Bets.CountAsync();
				var pendingBets = await _dbContext.Bets.CountAsync(b => b.Status == BetStatus.Pending);
				var wonBets = await _dbContext.Bets.CountAsync(b => b.Status == BetStatus.Won);
				var lostBets = await _dbContext.Bets.CountAsync(b => b.Status == BetStatus.Lost);

				var eventsVm = await BuildEventsSummaryAsync();

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
					Events = eventsVm
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
										? "Cancelled"
										: (m.LiveResult != null ? "Finished" : (m.MatchDate <= now ? "Live" : "Upcoming")),
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


