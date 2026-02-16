using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.Exceptions;
using MatchFixer.Core.ViewModels.Wallet;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure.Factories;
using Microsoft.EntityFrameworkCore;

using static MatchFixer.Common.GeneralConstants.WalletServiceConstants;

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

			if (amount <= 0)
				throw new ArgumentException(MoneyAmountMustBePositive);

			var wallet = await GetWalletAsync();

			if (wallet == null)
				throw new InvalidOperationException(WalletNotFound);

			if (wallet.IsLocked)
				throw new WalletLockedException();

			wallet.Balance += amount;
			wallet.UpdatedAt = DateTime.UtcNow;

			var transaction = WalletTransactionFactory.CreateDepositTransaction(wallet.Id, userId, amount, description);

			await _dbContext.WalletTransactions.AddAsync(transaction);
			await _dbContext.SaveChangesAsync();
		}

		public async Task<bool> WithdrawAsync(decimal amount, string description = null)
		{
			var userId = _userContextService.GetUserId();

			if (amount <= 0) throw new ArgumentException(MoneyAmountMustBePositive);

			var wallet = await GetWalletAsync();

			if (wallet == null) 
				throw new InvalidOperationException(WalletNotFound);

			if (wallet.IsLocked)
				throw new WalletLockedException();

			if (wallet.Balance < amount)
				return false;

			wallet.Balance -= amount;
			wallet.UpdatedAt = DateTime.UtcNow;

			var withdrawalTransaction =
				WalletTransactionFactory.CreateWithdrawalTransaction(wallet.Id, userId, amount, description);

			await _dbContext.WalletTransactions.AddAsync(withdrawalTransaction);
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
				return (false, WalletNotFound);

			var historyClearedAt = wallet.HistoryClearedAt;

			var visibleTransactions = historyClearedAt.HasValue
				? wallet.Transactions.Where(t => t.CreatedAt >= historyClearedAt.Value).ToList()
				: wallet.Transactions.ToList();

			if (!visibleTransactions.Any())
				return (false, NoTransactionsToClear);

			wallet.HistoryClearedAt = DateTime.UtcNow;
			await _dbContext.SaveChangesAsync();

			return (true, TransactionHistoryCleared);
		}

		public async Task<(bool Success, string Message)> DeductForBetAsync(Guid userId, decimal amount)
		{
			var wallet = await _dbContext.Wallets
				.Include(w => w.Transactions)
				.FirstOrDefaultAsync(w => w.UserId == userId);

			if (wallet == null)
				return (false, WalletNotFound);

			if (wallet.Balance < amount)
				return (false, InsufficientBalanceToPlaceBet);

			wallet.Balance -= amount;

			var transaction = WalletTransactionFactory.CreateBetPlacedTransaction(wallet.Id, userId, amount);

			await _dbContext.WalletTransactions.AddAsync(transaction);
			// intentionally does not save changes when adding the transaction, that is handled in the betting service only if all legs are valid 
			return (true, AmountDeductedForTheBet);
		}

		public async Task AwardWinningsAsync(Guid userId, decimal amount, string matchDescription)
		{
			if (amount <= 0)
				throw new ArgumentException(WinAmountMustBeGreaterThanZero);

			var wallet = await _dbContext.Wallets
				.FirstOrDefaultAsync(w => w.UserId == userId);

			if (wallet == null)
				throw new InvalidOperationException(WalletNotFound);

			wallet.Balance += amount;
			wallet.UpdatedAt = DateTime.UtcNow;

			var winningsTransaction =
				WalletTransactionFactory.CreateWinningsTransaction(wallet.Id, userId, amount, matchDescription);

			await _dbContext.WalletTransactions.AddAsync(winningsTransaction);
			await _dbContext.SaveChangesAsync();
		}

		public async Task<bool> AwardBirthdayBonusAsync(Guid userId)
		{
			const decimal bonusAmount = 10m;

			var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);

			wallet.Balance += bonusAmount;
			wallet.UpdatedAt = DateTime.UtcNow;

			var transaction = WalletTransactionFactory.CreateBirthdayBonus(wallet.Id, userId, bonusAmount);

			await _dbContext.WalletTransactions.AddAsync(transaction);
			await _dbContext.SaveChangesAsync();

			return true;
		}

		public async Task<bool> RefundBetAsync(Guid userId, decimal amount, Guid betSlipId)
		{
			var wallet = await _dbContext.Wallets
				.Include(w => w.Transactions)
				.FirstOrDefaultAsync(w => w.UserId == userId);

			if (wallet == null)
				return false;

			wallet.Balance += amount;

			var transaction =
				WalletTransactionFactory.CreateBetRefundedTransaction(wallet.Id, userId, amount, betSlipId);

			await _dbContext.WalletTransactions.AddAsync(transaction);

			return true;
		}

		public async Task<bool> HasTransactionForSlipAsync(
			Guid userId,
			WalletTransactionType type,
			Guid betSlipId)
		{
			return await _dbContext.WalletTransactions
				.AnyAsync(t =>
					t.TransactionType == type &&
					t.Reference.Contains(betSlipId.ToString()) &&
					t.Wallet.UserId == userId
				);
		}



	}
}

