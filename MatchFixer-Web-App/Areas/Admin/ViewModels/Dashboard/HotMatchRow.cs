namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Dashboard
{
	public class HotMatchRow
	{
		public Guid MatchId { get; set; }

		public string HomeTeam { get; set; }
		public string AwayTeam { get; set; }

		public string HomeLogo { get; set; } // API
		public string AwayLogo { get; set; } // API
		public string? HomeTeamLocalLogoUrl { get; set; } // LOCAL
		public string? AwayTeamLocalLogoUrl { get; set; } // LOCAL
		public string? EffectiveHomeTeamLogo =>
			!string.IsNullOrWhiteSpace(HomeTeamLocalLogoUrl)
				? HomeTeamLocalLogoUrl
				: HomeLogo;

		public string? EffectiveAwayTeamLogo =>
			!string.IsNullOrWhiteSpace(AwayTeamLocalLogoUrl)
				? AwayTeamLocalLogoUrl
				: AwayLogo;

		public int BetsCount { get; set; }
	}
}
