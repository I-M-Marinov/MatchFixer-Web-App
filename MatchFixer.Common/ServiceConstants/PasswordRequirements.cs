namespace MatchFixer.Common.ServiceConstants
{
	public static class PasswordRequirements
	{
		public const int MinLength = 8;
		public const bool RequireDigit = true;
		public const bool RequireLowercase = true;
		public const bool RequireUppercase = true;
		public const bool RequireNonAlphanumeric = true;
		public const string RegexPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";
		public const string ErrorMessage = "Password must be at least 8 characters and contain uppercase, lowercase, number, and special character.";
	}
}
