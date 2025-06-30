using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.Wallet;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer.Core.Services
{
	public class WalletService : IWalletService
	{
		private readonly MatchFixerDbContext _dbContext;
		private readonly IUserContextService _userContextService;
		private readonly ITimezoneService _timezoneService;

		public WalletService(MatchFixerDbContext dbContext, 
			IUserContextService userContextService,
			ITimezoneService timezoneService)
		{
			_dbContext = dbContext;
			_userContextService = userContextService;
			_timezoneService = timezoneService;
		}

		public async Task<bool> HasWalletAsync()
		{
			var userId = _userContextService.GetUserId();

			return await _dbContext.Wallets.AnyAsync(w => w.UserId == userId);
		}

		public async Task<Wallet> GetWalletAsync()
		{
			var userId = _userContextService.GetUserId();

			return await _dbContext.Wallets
				.Include(w => w.Transactions)
				.FirstOrDefaultAsync(w => w.UserId == userId);
		}

		public async Task<WalletViewModel> GetWalletViewModelAsync(string timeZoneId)
		{
			var userId = _userContextService.GetUserId();

			var wallet = await _dbContext.Wallets
				.Include(w => w.Transactions)
				.FirstOrDefaultAsync(w => w.UserId == userId);

			if (wallet == null)
				return null;

			var historyClearedAt = wallet.HistoryClearedAt;
			var filteredTransactions = historyClearedAt.HasValue
				? wallet.Transactions.Where(t => t.CreatedAt >= historyClearedAt.Value)
				: wallet.Transactions;

			var model = new WalletViewModel
			{
				Balance = wallet.Balance,
				Currency = wallet.Currency,
				Transactions = filteredTransactions
					.OrderByDescending(t => t.CreatedAt) // last transaction on top
					.Select(t => new WalletTransactionViewModel
					{
						CreatedAt = t.CreatedAt,
						DisplayTime = _timezoneService.FormatForUser(t.CreatedAt, timeZoneId, "en-US"),
						Amount = t.Amount,
						Description = t.Description,
						TransactionType = t.TransactionType
					}).ToList()
			};

			return model;
		}


		public async Task<Wallet> CreateWalletAsync()
		{
			var userId = _userContextService.GetUserId();

			var existing = await _dbContext.Wallets
				.FirstOrDefaultAsync(w => w.UserId == userId);

			if (existing != null)
				return existing;

			var wallet = new Wallet
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				Balance = 0m,
				Currency = "EUR",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _dbContext.Wallets.AddAsync(wallet);
			await _dbContext.SaveChangesAsync();

			return wallet;
		}

		public async Task DepositAsync(decimal amount, string description = null)
		{
			var userId = _userContextService.GetUserId();

			if (amount <= 0) throw new ArgumentException("Amount must be positive.");

			var wallet = await GetWalletAsync();

			if (wallet == null) throw new InvalidOperationException("Wallet not found.");

			wallet.Balance += amount;
			wallet.UpdatedAt = DateTime.UtcNow;

			await _dbContext.WalletTransactions.AddAsync(new WalletTransaction
			{
				Id = Guid.NewGuid(),
				WalletId = wallet.Id,
				Amount = amount,
				TransactionType = WalletTransactionType.Deposit,
				Reference = $"User: {userId} - Wallet: {wallet.Id} - Deposit",
				Description = description ?? "User deposit",
				CreatedAt = DateTime.UtcNow
			});

			await _dbContext.SaveChangesAsync();
		}

		public async Task<bool> WithdrawAsync(decimal amount, string description = null)
		{
			var userId = _userContextService.GetUserId();

			if (amount <= 0) throw new ArgumentException("Amount must be positive.");

			var wallet = await GetWalletAsync();
			if (wallet == null) throw new InvalidOperationException("Wallet not found.");

			if (wallet.Balance < amount)
				return false;

			wallet.Balance -= amount;
			wallet.UpdatedAt = DateTime.UtcNow;

			await _dbContext.WalletTransactions.AddAsync(new WalletTransaction
			{
				Id = Guid.NewGuid(),
				WalletId = wallet.Id,
				Amount = -amount,
				TransactionType = WalletTransactionType.Withdrawal,
				Reference = $"User: {userId} - Wallet: {wallet.Id} - Withdrawal",
				Description = description ?? "User withdrawal",
				CreatedAt = DateTime.UtcNow
			});

			await _dbContext.SaveChangesAsync();
			return true;
		}

		public async Task<(bool Success, string Message)> ClearTransactionHistoryAsync()
		{
			var userId = _userContextService.GetUserId();

			var wallet = await _dbContext.Wallets
				.Include(w => w.Transactions)
				.FirstOrDefaultAsync(w => w.UserId == userId);		

			if (wallet == null)
				return (false, "Wallet not found.");

			var historyClearedAt = wallet.HistoryClearedAt;

			var visibleTransactions = historyClearedAt.HasValue
				? wallet.Transactions.Where(t => t.CreatedAt >= historyClearedAt.Value).ToList()
				: wallet.Transactions.ToList();

			if (!visibleTransactions.Any())
				return (false, "No transactions to clear.");

			wallet.HistoryClearedAt = DateTime.UtcNow;
			await _dbContext.SaveChangesAsync();

			return (true, "Transaction history cleared.");
		}

		public async Task<(bool Success, string Message)> DeductForBetAsync(Guid userId, decimal amount)
		{
			var wallet = await _dbContext.Wallets
				.Include(w => w.Transactions)
				.FirstOrDefaultAsync(w => w.UserId == userId);

			if (wallet == null)
				return (false, "Wallet not found.");

			if (wallet.Balance < amount)
				return (false, "Insufficient balance to place the bet.");

			wallet.Balance -= amount;

			wallet.Transactions.Add(new WalletTransaction
			{
				Amount = -amount,
				CreatedAt = DateTime.UtcNow,
				Description = "Bet placed",
				TransactionType = WalletTransactionType.BetPlaced,
				Reference = $"User: {userId} - Wallet: {wallet.Id} - Bet Placed",
				WalletId = wallet.Id
			});

			await _dbContext.SaveChangesAsync();

			return (true, "Amount deducted for bet.");
		}

		public async Task AwardWinningsAsync(Guid userId, decimal amount, string matchDescription)
		{
			if (amount <= 0)
				throw new ArgumentException("Winning amount must be greater than zero.");

			var wallet = await _dbContext.Wallets
				.FirstOrDefaultAsync(w => w.UserId == userId);

			if (wallet == null)
				throw new InvalidOperationException("Wallet not found for the user.");

			wallet.Balance += amount;
			wallet.UpdatedAt = DateTime.UtcNow;

			var transaction = new WalletTransaction
			{
				Id = Guid.NewGuid(),
				WalletId = wallet.Id,
				Amount = amount,
				TransactionType = WalletTransactionType.Winnings,
				Reference = $"User: {userId} - Wallet: {wallet.Id} - Winnings",
				Description = $"Winnings from {matchDescription}",
				CreatedAt = DateTime.UtcNow
			};

			await _dbContext.WalletTransactions.AddAsync(transaction);
			await _dbContext.SaveChangesAsync();
		}


	}
}
