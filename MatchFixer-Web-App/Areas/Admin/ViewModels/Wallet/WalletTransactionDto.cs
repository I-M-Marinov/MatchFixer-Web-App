namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Wallet
{
	public class WalletTransactionDto
	{
		public DateTime CreatedUtc { get; set; }
		public string Type { get; set; } = "";
		public decimal Amount { get; set; }
		public decimal BalanceAfter { get; set; }
		public string? Note { get; set; }
	}
}
