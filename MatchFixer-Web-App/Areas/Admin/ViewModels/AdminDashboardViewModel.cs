using MatchFixer_Web_App.Areas.Admin.ViewModels.MatchEvents;

namespace MatchFixer_Web_App.Areas.Admin.ViewModels
{
	public class AdminDashboardViewModel
	{
		public int TotalUsers { get; set; }
		public int ActiveUsers { get; set; }
		public int DeletedUsers { get; set; }
		public int LockedUsers { get; set; }
		public int BannedUsers { get; set; }

		public decimal TotalWalletBalance { get; set; }
		public decimal TotalTransactions { get; set; }

		public int TotalBets { get; set; }
		public int PendingBets { get; set; }
		public int WonBets { get; set; }
		public int LostBets { get; set; }
		public AdminEventsSummaryViewModel Events { get; set; } = new();

	}
}
