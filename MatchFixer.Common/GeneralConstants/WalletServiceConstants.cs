namespace MatchFixer.Common.GeneralConstants
{
	public static class WalletServiceConstants
	{
		public const string MoneyAmountMustBePositive = "Amount must be positive.";
		public const string WalletNotFound = "Wallet was not found.";
		public const string NoTransactionsToClear = "No transactions to clear.";
		public const string TransactionHistoryCleared = "Transaction history was cleared.";
		public const string InsufficientBalanceToPlaceBet = "Insufficient balance to place the bet.";

		public const string WinAmountMustBeGreaterThanZero = "Amount won must be greater than zero.";
		public const string AmountDeductedForTheBet = "The amount was deducted for the bet.";

		// <<-------------- Wallet Controller -------------------->>

		public const string WalletCreatedSuccessfully = "Wallet created successfully.";
		public const string InsufficientBalanceForWithdrawal = "Insufficient balance for this withdrawal.";
		public static string SuccessfullyDeposited(decimal amount)
		{
			return $"Successfully deposited {amount:0.00} EUR.";
		}
		public static string SuccessfullyWithdrew(decimal amount)
		{
			return $"Successfully withdrew {amount:0.00} EUR.";
		}

							// Transaction constants

		public const string UserManualDeposit = "User manual deposit";
		public const string UserManualWithdrawal = "User manual withdrawal";

	}
}
