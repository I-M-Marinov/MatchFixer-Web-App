namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Events
{
	public class AdminTeamBettingStatsDto
	{
		public Guid TeamId { get; set; }   
		public string TeamName { get; set; }
		public string LogoUrl { get; set; }

		public int TotalBets { get; set; }
		public decimal TotalStake { get; set; }
		public decimal TotalPayout { get; set; }
		public decimal Profit => TotalStake - TotalPayout;
	}

}
