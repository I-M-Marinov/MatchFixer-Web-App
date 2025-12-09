namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Events
{
	public class AdminEventOverviewDto
	{
		public Guid EventId { get; set; }
		public string MatchName { get; set; }
		public string LeagueName { get; set; }
		public DateTime MatchDate { get; set; }
		public int? HomeScore { get; set; }
		public int? AwayScore { get; set; }
		public bool IsCancelled { get; set; }

		// Betting Summary
		public int TotalBets { get; set; }
		public decimal TotalStake { get; set; }
		public decimal TotalPayout { get; set; }
		public decimal Profit => TotalStake - TotalPayout;

		public List<AdminBetSummaryDto> Bets { get; set; } = new();
		public List<MatchEventLogDto> Logs { get; set; } = new();
	}
}
