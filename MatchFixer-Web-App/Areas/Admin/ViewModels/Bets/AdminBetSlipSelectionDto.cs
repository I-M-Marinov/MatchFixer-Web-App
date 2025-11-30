namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Bets
{
	public class AdminBetSlipSelectionDto
	{
		public string MatchName { get; set; }
		public string Pick { get; set; }
		public decimal Odds { get; set; }
		public string Status { get; set; }
		public string StatusBadge { get; set; } = "bg-light";
	}
}
