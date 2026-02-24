using MatchFixer.Common.Enums;
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

			var cutoff = wallet.HistoryClearedFromAdminAt;

			var txAsc = wallet.Transactions
				.Where(t => !cutoff.HasValue || t.CreatedAt >= cutoff.Value)
				.OrderBy(t => t.CreatedAt)
				.ThenBy(t => t.Id)
				.Select(t => new
				{
					t.Id,
					t.CreatedAt,
					TypeText = t.TransactionType,
					t.Amount,
					t.Description,
				})
				.ToList();


			decimal Signed(WalletTransactionType type, decimal amount)
			{
				var abs = Math.Abs(amount);
				return IsDebit(type) ? -abs : +abs;
			}


			var totalDelta = txAsc.Sum(t => Signed(t.TypeText, t.Amount));
			decimal running = wallet.Balance - totalDelta;

			var list = new List<WalletTransactionDto>(txAsc.Count);
			foreach (var t in txAsc)
			{
				var signed = Signed(t.TypeText, t.Amount);
				running += signed;

				list.Add(new WalletTransactionDto
				{
					Id = t.Id,
					CreatedUtc = DateTime.SpecifyKind(t.CreatedAt, DateTimeKind.Utc),
					DisplayTime = _timezoneService.FormatForUser(t.CreatedAt, timeZoneId, "en-US"),
					Type = t.TypeText.ToString() ?? string.Empty,
					Amount = signed,
					BalanceAfter = running,
					Note = t.Description,
				});
			}

			return new AdminWalletViewModel
			{
				UserId = userId,
				UserName = user?.UserName,
				Email = user?.Email,
				HasWallet = true,
				Balance = wallet.Balance,
				IsLocked = wallet.IsLocked,
				Currency = wallet.Currency,
				Transactions = list.OrderByDescending(x => x.CreatedUtc).ToList()
			};
		}



		public async Task<(bool Success, string Message)> CreateWalletForUserAsync(Guid userId, string currency = "EUR")
		{
			var existing = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
			if (existing != null) return (true, WalletAlreadyExists);

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
			return (true, WalletCreatedSuccessfully);
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

			var tx = WalletTransactionFactory.CreateDepositTransaction(wallet.Id, userId, amount, note);
			await _dbContext.WalletTransactions.AddAsync(tx);

			await _dbContext.SaveChangesAsync();
			return (true, DepositAddedSuccessfully);
		}

		public async Task<(bool Success, string Message)> AdminWithdrawAsync(Guid userId, decimal amount, string? description = null, Guid? adminActorId = null)
		{
			if (amount <= 0) return (false, MoneyAmountMustBePositive);

			var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
			if (wallet == null) return (false, WalletNotFound);
			if (wallet.Balance < amount) return (false, InsufficientBalanceToPlaceBet);

			wallet.Balance -= amount;
			wallet.UpdatedAt = DateTime.UtcNow;

			var note = AdminWithdrawalNote(description);

			var tx = WalletTransactionFactory.CreateWithdrawalTransaction(wallet.Id, userId, amount, note);
			await _dbContext.WalletTransactions.AddAsync(tx);

			await _dbContext.SaveChangesAsync();
			return (true, WithdrawalCompletedSuccessfully);
		}

		public async Task<(bool Success, string Message)> ClearTransactionHistoryAsync(Guid userId)
		{
			var wallet = await _dbContext.Wallets
				.Include(w => w.Transactions)
				.FirstOrDefaultAsync(w => w.UserId == userId);

			if (wallet == null) return (false, WalletNotFound);

			var historyClearedAt = wallet.HistoryClearedFromAdminAt;

			var visible = historyClearedAt.HasValue
				? wallet.Transactions.Where(t => t.CreatedAt >= historyClearedAt.Value)
				: wallet.Transactions;

			if (!visible.Any()) return (false, NoTransactionsToClear);

			wallet.HistoryClearedAt = DateTime.UtcNow;
			await _dbContext.SaveChangesAsync();
			return (true, TransactionHistoryCleared);
		}

		public async Task<bool> ToggleWalletLockAsync(Guid userId, string? reason = null)
		{
			var wallet = await _dbContext.Wallets
				.FirstOrDefaultAsync(w => w.UserId == userId);

			if (wallet == null)
				return false;

			wallet.IsLocked = !wallet.IsLocked;

			if (wallet.IsLocked)
			{
				wallet.LockedAtUtc = DateTime.UtcNow;
				wallet.ReasonForLock = reason;
			}
			else
			{
				wallet.LockedAtUtc = null;
				wallet.ReasonForLock = null;
			}

			await _dbContext.SaveChangesAsync();

			return true;
		}

		bool IsDebit(WalletTransactionType type)
		{
			return type switch
			{
				WalletTransactionType.BetPlaced or
				WalletTransactionType.Withdrawal or
				WalletTransactionType.AdminWithdrawal => true,

				_ => false
			};
		}

	}
}
