namespace MatchFixer_Web_App.Areas.Admin.ViewModels
{
	public class AdminUserRow
	{
		public Guid Id { get; set; }
		public string Email { get; set; } = "";
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public bool EmailConfirmed { get; set; }
		public bool IsLockedOut { get; set; }
		public DateTimeOffset? LockoutEnd { get; set; }
		public string[] Roles { get; set; } = Array.Empty<string>();
		public decimal? WalletBalance { get; set; }
		public int BetsCount { get; set; }
	}
}
