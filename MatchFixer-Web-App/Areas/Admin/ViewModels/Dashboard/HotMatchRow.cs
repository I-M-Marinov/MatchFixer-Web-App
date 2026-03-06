namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Dashboard
{
	public class HotMatchRow
	{
		public Guid MatchId { get; set; }

		public string HomeTeam { get; set; }
		public string AwayTeam { get; set; }

		public string HomeLogo { get; set; }
		public string AwayLogo { get; set; }

		public int BetsCount { get; set; }
	}
}
