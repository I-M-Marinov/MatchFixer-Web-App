namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Teams
{
	public class TeamSearchResult
	{
		public int ApiTeamId { get; init; }
		public string Name { get; init; } = "";
		public string LogoUrl { get; init; } = "";
		public int LeagueId { get; init; }
		public string LeagueName { get; init; } = "";
	}
}
