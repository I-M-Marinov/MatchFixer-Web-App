using MatchFixer.Core.ViewModels.Wallet;

namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Wallet
{
	public class AdminWalletViewModel
	{
		public Guid UserId { get; set; }
		public string? UserName { get; set; }
		public string? Email { get; set; }
		public bool HasWallet { get; set; }
		public decimal Balance { get; set; }
		public string Currency { get; set; } = "EUR";
		public List<WalletTransactionDto> Transactions { get; set; } = new();
	}
}
