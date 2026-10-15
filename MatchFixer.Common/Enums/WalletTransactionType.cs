namespace MatchFixer.Common.Enums
{
	public enum WalletTransactionType
	{
		Deposit = 0,
		Withdrawal = 1,
		BetPlaced = 2,
		Winnings = 3,
		Refund = 4,
		BirthdayBonus = 5,
		AdminWithdrawal = 6,
		AdminDeposit = 7,
		TournamentEntryFee = 8,   // real money charged to join a tournament
		TournamentPrize = 9,      // real money paid out to a tournament winner
		TournamentRefund = 10,    // real money refunded when a tournament is cancelled

	}

}
