namespace MatchFixer.Core.DTOs.Betting
{
	public enum BettingBlockReason
	{
		None = 0,
		NotSignedIn = 1,
		AgeNotVerified = 2,
		Underage = 3,
		BlockedByAdmin = 4
	}

	public sealed record BettingEligibility(bool CanBet, BettingBlockReason Reason, string Message)
	{
		public static readonly BettingEligibility Allowed = new(true, BettingBlockReason.None, string.Empty);
	}
}
