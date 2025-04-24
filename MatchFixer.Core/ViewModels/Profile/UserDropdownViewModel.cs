namespace MatchFixer_Web_App.Models.Profile
{
	public class UserDropdownViewModel
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; } = null!;
		public string LastName { get; set; } = null!;
		public string FullName => $"{FirstName} {LastName}";
		public string? ProfileImageUrl { get; set; }
	}

}
