namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Trophy
{
	public class AdminUserTrophyViewModel
	{
		public Guid UserId { get; set; }
		public string Email { get; set; }
		public string FullName { get; set; }

		public List<AdminTrophyItemViewModel> Trophies { get; set; } = new();
	}

}
