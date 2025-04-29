namespace MatchFixer.Core.ViewModels.Profile
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
