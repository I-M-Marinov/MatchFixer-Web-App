namespace MatchFixer_Web_App.Areas.Admin.ViewModels
{
	public class AdminUsersListViewModel
	{
		public string? Query { get; set; }
		public int Page { get; set; }
		public int PageSize { get; set; }
		public int Total { get; set; }
		public List<AdminUserRow> Users { get; set; } = new();
	}
}
