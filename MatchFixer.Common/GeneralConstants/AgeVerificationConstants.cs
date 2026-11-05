namespace MatchFixer.Common.GeneralConstants
{
	public static class AgeVerificationConstants
	{
		public const int MinimumBettingAge = 18;

		public const string MsgNotSignedIn = "Please sign in to place bets.";
		public const string MsgAgeNotVerified = "Please verify your date of birth before placing bets.";
		public const string MsgUnderage = "You must be 18 or older to place bets.";
		public const string MsgBlockedByAdmin = "Betting has been disabled on your account. Please contact support.";
		public const string MsgInvalidDob = "Please enter a valid date of birth.";
		public const string MsgAlreadyVerified = "Your age is already verified.";
		public const string MsgVerifiedSuccessfully = "Thanks! Your age is verified and you can now place bets.";
		public const string MsgUserNotFound = "The user was not found.";
		public const string MsgAdminNeedsDob = "Enter a valid date of birth to verify this user.";
		public const string MsgDobChangeSubmitted = "You already have a date of birth on file, so your new date has been sent to an admin for approval.";
	}
}
