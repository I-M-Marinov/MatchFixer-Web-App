namespace MatchFixer.Common.FootballCompetitions
{
		public static class FootballCompetitionLogos
		{
			// Domestic leagues
			public static readonly Dictionary<string, int> LeagueIds =
				new(StringComparer.OrdinalIgnoreCase)
				{
					["Premier League"] = 39,
					["Parva Liga"] = 172,
					["Bundesliga"] = 78,
					["La Liga"] = 140,
					["Eredivisie"] = 88,
					["Ligue 1"] = 61,
					["Serie A"] = 135,
					["Liga Portugal"] = 94,
					["Swiss League"] = 207,
					["Polish League Ekstraklasa"] = 106
				};

			// Competitions
			public static readonly Dictionary<string, int> CompetitionIds =
				new(StringComparer.OrdinalIgnoreCase)
				{
					["UEFA Champions League"] = 2,
					["UEFA Europa League"] = 3,
					["UEFA Europa Conference League"] = 848
				};

			public static string? GetLeagueLogo(string leagueName)
			{
				if (LeagueIds.TryGetValue(leagueName, out var id))
					return $"https://media.api-sports.io/football/leagues/{id}.png";

				return null;
			}

			public static string? GetCompetitionLogo(string competitionName)
			{
				if (CompetitionIds.TryGetValue(competitionName, out var id))
					return $"https://media.api-sports.io/football/leagues/{id}.png";

				return null;
			}
		}
}


