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
			{ 172, "First Professional League (Bulgaria)" }
		};
	}
}