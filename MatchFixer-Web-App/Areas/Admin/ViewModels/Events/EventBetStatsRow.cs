namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Events
{
	public class EventBetStatsRow
	{
		public Guid EventId { get; set; }
		public string HomeTeam { get; set; } = "";
		public string AwayTeam { get; set; } = "";	
		public string HomeTeamLogoUrl { get; set; } = "";
		public string AwayTeamLogoUrl { get; set; } = "";
		public string LeagueName { get; set; } = "";
		public DateTime? KickoffUtc { get; set; }

		// amounts
		public decimal TotalStake { get; set; }
		public decimal HomeStake { get; set; }
		public decimal DrawStake { get; set; }
		public decimal AwayStake { get; set; }

		// counts 
		public int TotalBets { get; set; }
		public int HomeBets { get; set; }
		public int DrawBets { get; set; }
		public int AwayBets { get; set; }

		// % (0..100)
		public decimal HomePct { get; set; }
		public decimal DrawPct { get; set; }
		public decimal AwayPct { get; set; }
	}
}
