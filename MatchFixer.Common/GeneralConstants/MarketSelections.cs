namespace MatchFixer.Common.GeneralConstants
{
	/// <summary>
	/// Selection codes for the extra betting markets. Views, JS and settlement all use these exact strings.
	/// The Match Result (1X2) market keeps using the MatchPick enum.
	/// </summary>
	public static class MarketSelections
	{
		public const string Over = "Over";
		public const string Under = "Under";

		public const string Yes = "Yes";
		public const string No = "No";

		public const string HomeOrDraw = "1X";
		public const string HomeOrAway = "12";
		public const string DrawOrAway = "X2";

		public const decimal DefaultGoalsLine = 2.5m;

		public static readonly string[] OverUnder = { Over, Under };
		public static readonly string[] BothTeamsToScore = { Yes, No };
		public static readonly string[] DoubleChance = { HomeOrDraw, HomeOrAway, DrawOrAway };
	}
}
