namespace MatchFixer_Web_App.Areas.Admin.ViewModels.MatchEvents
{
	public class AdminEventRow
	{
		public Guid Id { get; set; }
		public string? League { get; set; } // for later use ... 
		public string HomeTeam { get; set; } = "";
		public string HomeTeamLogo { get; set; } = "";
		public string AwayTeam { get; set; } = "";
		public string AwayTeamLogo { get; set; } = "";
		public DateTime? MatchUtc { get; set; }
		public string Status { get; set; } = "";
		public DateTime? UpdatedUtc { get; set; }
	}
}
