namespace MatchFixer.Common.FootballCompetitions
{
	public static class FootballCompetitions
	{
		public const string ChampionsLeague = "UEFA Champions League";
		public const string EuropaLeague = "UEFA Europa League";
		public const string ConferenceLeague = "UEFA Europa Conference League";

		// Maps an API-Football league id to the internal competition tag.
		// Domestic leagues are intentionally absent (they stay untagged).
		public static readonly Dictionary<int, string> ByApiLeagueId = new()
		{
			{ 2,   ChampionsLeague },
			{ 3,   EuropaLeague },
			{ 848, ConferenceLeague }
		};
	}
}
