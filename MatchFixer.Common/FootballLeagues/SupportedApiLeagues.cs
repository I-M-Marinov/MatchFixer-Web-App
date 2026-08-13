namespace MatchFixer.Common.FootballLeagues
{
	public static class SupportedApiLeagues
	{
		public static readonly Dictionary<int, string> Football = new()
		{
			// Top 5
			{ 39,  "Premier League (England)" },
			{ 140, "La Liga (Spain)" },
			{ 135, "Serie A (Italy)" },
			{ 78,  "Bundesliga (Germany)" },
			{ 61,  "Ligue 1 (France)" },

			// European leagues
			{ 88,  "Eredivisie (Netherlands)" },
			{ 94,  "Primeira Liga (Portugal)" },
			{ 106, "Ekstraklasa (Poland)" },
			{ 207, "Super League (Switzerland)" },

			// Regional
			{ 172, "First Professional League (Bulgaria)" },
			{ 173, "Second Professional League (Bulgaria)" },

			// European competitions (UEFA)
			{ 2,   "UEFA Champions League" },
			{ 3,   "UEFA Europa League" },
			{ 848, "UEFA Conference League" }
		};

		// Logo overrides for leagues where the API Sports image is wrong or missing
		public static readonly Dictionary<int, string> LogoOverrides = new()
		{
			{ 173, "https://bulgarian-football.com/files/logos/vtora-liga_226.png" }
		};

		// Leagues where NS status is unreliable
		public static readonly HashSet<int> UnreliableNsStatusLeagues = new()
		{
			172 // Bulgaria
		};
	}
}