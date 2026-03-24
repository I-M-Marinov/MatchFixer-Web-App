namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Events
{
	public class AdminBetSummaryDto
	{
		public Guid BetId { get; set; }
		public string Username { get; set; }
		public string FullName { get; set; }

		public string Pick { get; set; }
		public decimal Odds { get; set; }
		public decimal Stake { get; set; }

		public string BetStatus { get; set; } = null!;      //  leg status
		public string BetSlipStatus { get; set; } = null!;  // bet slip status

        public decimal? Payout { get; set; }
		public int Legs { get; set; }
	}

}