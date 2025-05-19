namespace MatchFixer.Common.ServiceConstants
{
	public static class UserCleanupConstants
	{
		public static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(30);
		public static readonly TimeSpan AccountExpiration = TimeSpan.FromHours(1);
	}
}
