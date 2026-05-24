namespace MatchFixer.Core.ViewModels.Index
{
	public class RecentBigWinViewModel
	{
		public Guid BetSlipId { get; set; }

		public string Username { get; set; } = null!;

		public decimal WinAmount { get; set; }

		public decimal StakeAmount { get; set; }

		public int LegsCount { get; set; }

		public decimal TotalOdds { get; set; }

		public List<decimal> Odds { get; set; } = new();

		public DateTime BetTime { get; set; }

		public List<RecentBigWinBetViewModel> Bets { get; set; } = new();
	}
}
