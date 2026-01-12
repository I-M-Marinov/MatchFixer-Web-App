namespace MatchFixer.Common.GeneralConstants
{
	public static class BettingServiceConstants
	{
		public const string NoBetsProvided = "No bets provided.";
		public const string BetAmountMustBeGreaterThanZero = "Bet amount must be greater than zero.";
		public const string EventOrEventsCancelledInSlip = "An event or events in your slip was cancelled ! Choose other events to bet on.";

		public const string EventCancelledInSlip = "One or more events in your slip were cancelled.";
		public const string EventPostponedInSlip = "One or more events in your slip were postponed.";
		public const string EventAlreadyStartedInSlip = "One or more events in your slip have already started.";
		public const string EventAlreadyFinishedInSlip = "One or more events in your slip have already finished.";
		public const string EventNotOpenForBetting = "One or more events are not open for betting.";

		public const string BetSlipSubmittedSuccessfully = "Your betting slip was successfully submitted! Good Luck!";
		public const string BoostAlreadyUsedMaximumTimesForEvent = "Boost already used maximum times for this event.";

		public static string MaxStakePerBetIs(decimal? maxStakePerBet)
		{
			return $"Max stake per bet is {maxStakePerBet}.";
		}	
		public static string InvalidPickOption(string selectedOption)
		{
			return $"Invalid pick option '{selectedOption}'.";
		}
		public static string InvalidMatchId(Guid matchId)
		{
			return $"Match with ID {matchId} not found.";
		}
	}
}
