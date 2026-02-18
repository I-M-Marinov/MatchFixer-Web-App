using MatchFixer.Common.Enums;
using MatchFixer.Infrastructure.Entities;

namespace MatchFixer.Infrastructure.Factories
{
	public static class WalletTransactionFactory
	{
		public static WalletTransaction CreateTransaction(
			Guid walletId,
			decimal amount,
			WalletTransactionType transactionType,
			string description,
			string reference)
		{
			return new WalletTransaction
			{
				Id = Guid.NewGuid(),
				WalletId = walletId,
				Amount = amount,
				TransactionType = transactionType,


				Description = description,
				Reference = reference,
				CreatedAt = DateTime.UtcNow
			};
		}

		public static WalletTransaction CreateBirthdayBonus(Guid walletId, Guid userId, decimal amount)
		{
			return CreateTransaction(
				walletId,
				amount,
				WalletTransactionType.BirthdayBonus,
				"🎂 Birthday Bonus",
				$"User: {userId} - Birthday bonus granted"
			);
		}

		public static WalletTransaction CreateWinningsTransaction(Guid walletId, Guid userId, decimal amount, string betslipDescription)
		{
			return new WalletTransaction
			{
				Id = Guid.NewGuid(),
				WalletId = walletId,
				Amount = amount,
				TransactionType = WalletTransactionType.Winnings,
				Reference = $"User: {userId} - Wallet: {walletId} - Winnings",
				Description = $"Winnings from {betslipDescription}",
				CreatedAt = DateTime.UtcNow
			};
		}

		public static WalletTransaction CreateWithdrawalTransaction(Guid walletId, Guid userId, decimal amount, string? description = null)
		{
			return new WalletTransaction
			{
				Id = Guid.NewGuid(),
				WalletId = walletId,
				Amount = -amount, 
				TransactionType = WalletTransactionType.Withdrawal,
				Reference = $"User: {userId} - Wallet: {walletId} - Withdrawal",
				Description = description ?? "User withdrawal",
				CreatedAt = DateTime.UtcNow
			};
		}

		public static WalletTransaction CreateBetPlacedTransaction(Guid walletId, Guid userId, decimal amount, Guid betslipId)
		{
			return new WalletTransaction
			{
				Id = Guid.NewGuid(),
				WalletId = walletId,
				Amount = -amount, 
				TransactionType = WalletTransactionType.BetPlaced,
				Description = $"Bet placed for slip # {betslipId}",
				Reference = $"User: {userId} - Wallet: {walletId} - Bet Placed",
				CreatedAt = DateTime.UtcNow
			};
		}

		public static WalletTransaction CreateDepositTransaction(Guid walletId, Guid userId, decimal amount, string? description = null)
		{
			return new WalletTransaction
			{
				Id = Guid.NewGuid(),
				WalletId = walletId,
				Amount = amount, 
				TransactionType = WalletTransactionType.Deposit,
				Reference = $"User: {userId} - Wallet: {walletId} - Deposit",
				Description = description ?? "User deposit",
				CreatedAt = DateTime.UtcNow
			};
		}

		public static WalletTransaction CreateBetRefundedTransaction(Guid walletId, Guid userId, decimal amount, Guid betSlipId, string? description = null)
		{
			return new WalletTransaction
			{
				Id = Guid.NewGuid(),
				WalletId = walletId,
				Amount = amount,
				TransactionType = WalletTransactionType.Refund, 
				Reference = $"User: {userId} - Wallet: {walletId} - Refund for BetSlip: {betSlipId}",
				Description = description ?? $"Refund for cancelled bet slip ({betSlipId})",
				CreatedAt = DateTime.UtcNow
			};
		}
	}

}
