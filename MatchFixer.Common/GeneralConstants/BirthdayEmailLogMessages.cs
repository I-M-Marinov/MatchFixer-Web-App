namespace MatchFixer.Common.GeneralConstants
{
	public static class BirthdayEmailLogMessages
	{
		// Service lifecycle
		public const string ServiceStarted = "BirthdayEmailService started.";
		public const string ServiceStopping = "BirthdayEmailService stopping.";

		// Processing
		public const string NoBirthdaysToday = "No birthdays today.";

		// Email
		public const string BirthdayEmailSent = "Sent birthday email to {Email}.";

		// Wallet
		public const string BirthdayBonusAwarded = "Awarded €10 birthday bonus to {Email}.";

		// Errors
		public const string ProcessingError = "Error occurred while sending birthday emails.";
	}
}
