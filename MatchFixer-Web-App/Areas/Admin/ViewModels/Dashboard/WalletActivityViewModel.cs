namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Dashboard
{
	public class WalletActivityViewModel
	{
		public decimal DepositsToday { get; set; }
		public decimal WithdrawalsToday { get; set; }
		public decimal NetFlow => DepositsToday - WithdrawalsToday;
	}
}
