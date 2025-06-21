namespace MatchFixer.Core.ViewModels.Wallet
{
	public class WalletViewModel
	{
		public decimal Balance { get; set; }
		public string Currency { get; set; }

		public List<WalletTransactionViewModel> Transactions { get; set; }
	}
}
