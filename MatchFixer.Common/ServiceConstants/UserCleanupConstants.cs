namespace MatchFixer.Common.ServiceConstants
{
	public static class UserCleanupConstants
	{
		public static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(24);
		public static readonly TimeSpan AccountExpiration = TimeSpan.FromMinutes(30);
	}
}
