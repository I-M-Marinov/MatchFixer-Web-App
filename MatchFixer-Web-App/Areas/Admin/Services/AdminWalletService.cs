using MatchFixer.Core.ViewModels.Wallet;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure.Factories;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Wallet;
using Microsoft.EntityFrameworkCore;
using static MatchFixer.Common.GeneralConstants.WalletServiceConstants;

namespace MatchFixer_Web_App.Areas.Admin.Services
{
	public class AdminWalletService : IAdminWalletService
	{
		private readonly MatchFixerDbContext _dbContext;
		private readonly ITimezoneService _timezoneService;

		public AdminWalletService(MatchFixerDbContext dbContext, ITimezoneService timezoneService)
		{
			_dbContext = dbContext;
			_timezoneService = timezoneService;
		}

		public async Task<bool> UserHasWalletAsync(Guid userId)
			=> await _dbContext.Wallets.AnyAsync(w => w.UserId == userId);

		public async Task<Wallet?> GetWalletAsync(Guid userId, bool includeTransactions = true)
		{
			var q = _dbContext.Wallets.AsQueryable();
			if (includeTransactions) q = q.Include(w => w.Transactions);
			return await q.FirstOrDefaultAsync(w => w.UserId == userId);
		}

		public async Task<AdminWalletViewModel?> GetWalletViewModelAsync(Guid userId, string timeZoneId)
		{
			var wallet = await _dbContext.Wallets
				.Include(w => w.Transactions)
				.FirstOrDefaultAsync(w => w.UserId == userId);

			if (wallet == null) return null;

			var user = await _dbContext.Users
				.AsNoTracking()
				.Where(u => u.Id == userId)
				.Select(u => new { u.UserName, u.Email })
				.FirstOrDefaultAsync();

			var historyClearedAt = wallet.HistoryClearedAt;
			var txQuery = historyClearedAt.HasValue
				? wallet.Transactions.Where(t => t.CreatedAt >= historyClearedAt.Value)
				: wallet.Transactions;

			var vm = new AdminWalletViewModel
			{
				UserId = userId,
				UserName = user?.UserName,
				Email = user?.Email,
				HasWallet = true,
				Balance = wallet.Balance,
				Currency = wallet.Currency,
				Transactions = txQuery
					.OrderByDescending(t => t.CreatedAt)
					.Select(t => new WalletTransactionViewModel
					{
						CreatedAt = t.CreatedAt,
						DisplayTime = _timezoneService.FormatForUser(t.CreatedAt, timeZoneId, "en-US"),
						Amount = t.Amount,
						Description = t.Description,
						TransactionType = t.TransactionType
					})
					.ToList()
			};

			return vm;
		}

		public async Task<(bool Success, string Message)> CreateWalletForUserAsync(Guid userId, string currency = "EUR")
		{
			var existing = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
			if (existing != null) return (true, "Wallet already exists.");

			var wallet = new Wallet
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				Balance = 0m,
				Currency = currency,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _dbContext.Wallets.AddAsync(wallet);
			await _dbContext.SaveChangesAsync();
			return (true, "Wallet created.");
		}

		public async Task<(bool Success, string Message)> AdminDepositAsync(Guid userId, decimal amount, string? description = null, Guid? adminActorId = null)
		{
			if (amount <= 0) return (false, MoneyAmountMustBePositive);

			var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
			if (wallet == null) return (false, WalletNotFound);

			wallet.Balance += amount;
			wallet.UpdatedAt = DateTime.UtcNow;

			var note = string.IsNullOrWhiteSpace(description)
				? "Admin deposit"
				: $"Admin deposit: {description}";

			// If you have a dedicated factory for admin adjustments, swap this call accordingly.
			var tx = WalletTransactionFactory.CreateDepositTransaction(wallet.Id, userId, amount, note);
			await _dbContext.WalletTransactions.AddAsync(tx);

			await _dbContext.SaveChangesAsync();
			return (true, "Deposit added.");
		}

		public async Task<(bool Success, string Message)> AdminWithdrawAsync(Guid userId, decimal amount, string? description = null, Guid? adminActorId = null)
		{
			if (amount <= 0) return (false, MoneyAmountMustBePositive);

			var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
			if (wallet == null) return (false, WalletNotFound);
			if (wallet.Balance < amount) return (false, InsufficientBalanceToPlaceBet);

			wallet.Balance -= amount;
			wallet.UpdatedAt = DateTime.UtcNow;

			var note = string.IsNullOrWhiteSpace(description)
				? "Admin withdrawal"
				: $"Admin withdrawal: {description}";

			var tx = WalletTransactionFactory.CreateWithdrawalTransaction(wallet.Id, userId, amount, note);
			await _dbContext.WalletTransactions.AddAsync(tx);

			await _dbContext.SaveChangesAsync();
			return (true, "Withdrawal completed.");
		}

		public async Task<(bool Success, string Message)> ClearTransactionHistoryAsync(Guid userId)
		{
			var wallet = await _dbContext.Wallets
				.Include(w => w.Transactions)
				.FirstOrDefaultAsync(w => w.UserId == userId);

			if (wallet == null) return (false, WalletNotFound);

			var historyClearedAt = wallet.HistoryClearedAt;
			var visible = historyClearedAt.HasValue
				? wallet.Transactions.Where(t => t.CreatedAt >= historyClearedAt.Value)
				: wallet.Transactions;

			if (!visible.Any()) return (false, NoTransactionsToClear);

			wallet.HistoryClearedAt = DateTime.UtcNow;
			await _dbContext.SaveChangesAsync();
			return (true, TransactionHistoryCleared);
		}
	}
}
