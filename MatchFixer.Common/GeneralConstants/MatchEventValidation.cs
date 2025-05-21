namespace MatchFixer.Common.GeneralConstants
{
	public static class MatchEventValidation
	{
		public const string HomeTeamCannotExceed100Characters = $"The home team name cannot exceed 100 characters.";
		public const int HomeTeamMaxLength = 100;	
		
		public const string AwayTeamCannotExceed100Characters = "The away team name cannot exceed 100 characters.";
		public const int AwayTeamMaxLength = 100;

		public const double OddsMinValue = 1.01;
		public const double OddsMaxValue = 1000;
	}
}
