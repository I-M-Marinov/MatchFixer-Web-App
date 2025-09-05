using MatchFixer.Common.Enums;
using MatchFixer.Infrastructure;
using MatchFixer_Web_App.Areas.Admin.ViewModels;
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
				var totalUsers = await _dbContext.Users.CountAsync();
				var activeUsers = await _dbContext.Users.CountAsync(u => u.EmailConfirmed);

				var totalWalletBalance = await _dbContext.Wallets.SumAsync(w => (decimal?)w.Balance) ?? 0m;
				var totalTransactions = await _dbContext.WalletTransactions.CountAsync();

				var totalBets = await _dbContext.Bets.CountAsync();
				var pendingBets = await _dbContext.Bets.CountAsync(b => b.Status == BetStatus.Pending);
				var wonBets = await _dbContext.Bets.CountAsync(b => b.Status == BetStatus.Won);
				var lostBets = await _dbContext.Bets.CountAsync(b => b.Status ==BetStatus.Lost);

				return new AdminDashboardViewModel
				{
					TotalUsers = totalUsers,
					ActiveUsers = activeUsers,
					TotalWalletBalance = totalWalletBalance,
					TotalTransactions = totalTransactions,
					TotalBets = totalBets,
					PendingBets = pendingBets,
					WonBets = wonBets,
					LostBets = lostBets
				};
			}
		}
	}

}
