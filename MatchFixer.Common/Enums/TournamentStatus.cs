namespace MatchFixer.Common.Enums
{
	public enum TournamentStatus
	{
		Upcoming = 0,   // created, registration open, not yet started
		Running = 1,    // started; entrants can place bankroll bets
		Finished = 2,   // ended and prize pool settled
		Cancelled = 3   // ended without running (e.g. not enough entrants) - entry fees refunded
	}
}
