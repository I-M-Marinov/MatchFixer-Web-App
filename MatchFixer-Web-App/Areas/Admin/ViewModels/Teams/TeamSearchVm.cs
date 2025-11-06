namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Teams
{
	public class TeamSearchVm
	{
		public string? Query { get; set; }
		public int LeagueId { get; set; }
		public int Season { get; set; }
		public IReadOnlyDictionary<int, string>? Leagues { get; set; }
		public IReadOnlyList<TeamSearchResult> Results { get; set; } = Array.Empty<TeamSearchResult>();
		public int[] SelectedLeagueIds { get; set; } = Array.Empty<int>();

	}
}

