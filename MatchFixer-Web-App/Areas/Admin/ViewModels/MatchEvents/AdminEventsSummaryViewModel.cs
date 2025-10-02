namespace MatchFixer_Web_App.Areas.Admin.ViewModels.MatchEvents
{
	public class AdminEventsSummaryViewModel
	{
		// Totals
		public int TotalEvents { get; set; }
		public int Upcoming { get; set; }
		public int Live { get; set; }
		public int Finished { get; set; }

		// Optional (show only if you have the fields)
		public int Canceled { get; set; }
		public int Postponed { get; set; }

		// Bets-related
		public int EventsWithVoidedBets { get; set; }

		// Time windows
		public int Today { get; set; }
		public int Last7Days { get; set; }

		// Recently updated events (last N)
		public List<AdminEventRow> RecentUpdated { get; set; } = new();
	}
}
