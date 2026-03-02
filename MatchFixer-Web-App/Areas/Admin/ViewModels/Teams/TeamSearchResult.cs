namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Teams
{
	public class TeamSearchResult
	{
		public int ApiTeamId { get; init; }
		public string Name { get; init; } = "";
		public string LogoUrl { get; init; } = "";          // API logo
		public string? LocalLogoUrl { get; init; }         // Local stored logo
		public string EffectiveLogoUrl =>
			!string.IsNullOrWhiteSpace(LocalLogoUrl)
				? LocalLogoUrl
				: LogoUrl;

		public int LeagueId { get; init; }
		public string LeagueName { get; init; } = "";
	}
}
