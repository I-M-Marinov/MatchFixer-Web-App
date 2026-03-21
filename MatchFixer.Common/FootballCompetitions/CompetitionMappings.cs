namespace MatchFixer.Common.FootballCompetitions
{
	using MatchFixer.Common.VirtualLeagues;
	public static class CompetitionMappings
	{
		public static readonly Dictionary<string, int> NameToId = new()
		{
			// UEFA
			{ FootballCompetitions.ChampionsLeague, 1001 },
			{ FootballCompetitions.EuropaLeague, 1002 },
			{ FootballCompetitions.ConferenceLeague, 1003 },

			// International
			{ VirtualLeagues.InternationalName, VirtualLeagues.InternationalId },
			{ VirtualLeagues.WorldCupName, VirtualLeagues.WorldCupId },
			{ VirtualLeagues.EuroName, VirtualLeagues.EuroId }
		};
	}
}
