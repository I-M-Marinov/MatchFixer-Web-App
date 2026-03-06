namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Dashboard
{
	public class TopUserWinLossRow
	{
		public Guid UserId { get; set; }
		public string Username { get; set; }
		public decimal Profit { get; set; }
		public string Email { get; set; }
	}
}
