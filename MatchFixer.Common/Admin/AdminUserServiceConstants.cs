namespace MatchFixer.Common.Admin
{
	public static class AdminUserServiceConstants
	{
		// User status filter keys
		public const string StatusActive = "active";
		public const string StatusUnconfirmed = "unconfirmed";
		public const string StatusLocked = "locked";
		public const string StatusDeleted = "deleted";
		public const string StatusAll = "all";

		public const string CannotLockOwnAccount = "You cannot lock your own account.";
		public const string UserWasNotFound = "The user was not found.";
		public const string FailedToEnableLockForUser = "Failed to enable lockout for this user.";
		public const string UserLocked = "User locked.";
		public const string UserUnlocked = "User unlocked.";
		public const string FailedToLockUser = "Failed to lock user.";
		public const string FailedTotoClearLockOut = "Failed to clear lockout end date.";

		public static string UserLockedUntil(DateTimeOffset? currentEnd)
		{
			return $"User is already locked until {currentEnd.Value:yyyy-MM-dd HH:mm} UTC.";
		}

	}
}
