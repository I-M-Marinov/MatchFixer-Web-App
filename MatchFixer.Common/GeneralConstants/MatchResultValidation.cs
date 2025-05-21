namespace MatchFixer.Common.GeneralConstants
{
	public static class MatchResultValidation
	{
		public const string LeagueNameCannotExceed100Characters = "League name cannot exceed 100 characters.";
		public const byte LeagueNameMaxLength = 100;

		public const string YearMustBeValid = "Season must be a valid year.";
		public const int YearMinValue = 1925;
		public const int YearMaxValue = 2100;

		public const string HomeTeamCannotExceed100Characters = "Home team name cannot exceed 100 characters.";
		public const byte HomeTeamMaxLength = 100;

		public const string HomeTeamLogoUrlCannotExceed300Characters = "Home team logo URL cannot exceed 300 characters.";
		public const int HomeTeamLogoUrlMaxLength = 300;
		
		public const string AwayTeamCannotExceed100Characters = "Away team name cannot exceed 100 characters.";
		public const byte AwayTeamMaxLength = 100;

		public const string AwayTeamLogoUrlCannotExceed300Characters = "Away team logo URL cannot exceed 300 characters.";
		public const int AwayTeamLogoUrlMaxLength = 300;

		public const string ScoreMustBeValid = "Score must be between 0 and 99.";
		public const int ScoreMinValue = 0;
		public const int ScoreMaxValue = 99;
	}
}
