using MatchFixer.Common.Enums;
using MatchFixer.Common.ServiceConstants;

namespace MatchFixer.Common.FootballCompetitions
{
	public static class FootballLeagueMappings
	{
		// =========================
		// Domestic leagues
		// =========================
		public static readonly Dictionary<InternalLeague, LeagueExternalIds> Leagues =
			new()
			{
				[InternalLeague.PremierLeague] = new()
				{
					ApiFootballLeagueId = 39,
					TheSportsDbLeagueId = 4328
				},
				[InternalLeague.Bundesliga] = new()
				{
					ApiFootballLeagueId = 78,
					TheSportsDbLeagueId = 4331
				},
				[InternalLeague.LaLiga] = new()
				{
					ApiFootballLeagueId = 140,
					TheSportsDbLeagueId = 4335
				},
				[InternalLeague.Eredivisie] = new()
				{
					ApiFootballLeagueId = 88,
					TheSportsDbLeagueId = 4337
				},
				[InternalLeague.Ligue1] = new()
				{
					ApiFootballLeagueId = 61,
					TheSportsDbLeagueId = 4334
				},
				[InternalLeague.SerieA] = new()
				{
					ApiFootballLeagueId = 135,
					TheSportsDbLeagueId = 4332
				},
				[InternalLeague.LigaPortugal] = new()
				{
					ApiFootballLeagueId = 94,
					TheSportsDbLeagueId = 4344
				}
			};
	}
}
