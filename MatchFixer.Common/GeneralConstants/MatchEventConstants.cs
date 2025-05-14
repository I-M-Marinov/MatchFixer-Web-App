namespace MatchFixer.Common.GeneralConstants
{
	public static class MatchEventConstants
	{
		public const string TeamDoesNotExist = "Home or Away team does not exist!";
		public const string FailedToGetLiveEvents = "Failed to get the live events!";

		public const string MatchUpdateFailed = "Failed to edit the match. It might not exist or is already cancelled.";
		public const string MatchUpdateSuccessful = "Match updated successfully.";

		public const string MatchEventWasSuccessfullyAdded = "Match was successfully added to the events board!";

		public const string EventCancellationUnsuccessful = "There was an error cancelling this event";
		public const string EventCancellationSuccessful = "Event cancelled successfully.";

		public const string TeamNotFoundInDatabase = "Team '{0}' not found in database. Seed teams first.";
		public const string MatchNotFound = "Match not found."; 
		public const string MatchHasBeenPostponed = "Match has been postponed.";

		// Full Time Constants
		public const string UnableToMarkMatchAsFullTime = "Unable to mark match as Full Time.";
		public const string MatchMarkedAsFullTime = "Match successfully marked as Full Time.";

	}
}
