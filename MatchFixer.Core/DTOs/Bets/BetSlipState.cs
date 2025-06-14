namespace MatchFixer.Core.DTOs.Bets
{
	public class BetSlipState
	{
		public string UserId { get; set; } // optional for logged-in
		public List<BetSlipItem> Bets { get; set; } = new List<BetSlipItem>();
		public decimal TotalOdds { get; set; }
		public decimal StakeAmount { get; set; }
	}
}
