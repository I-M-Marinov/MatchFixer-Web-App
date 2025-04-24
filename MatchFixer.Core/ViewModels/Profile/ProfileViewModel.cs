namespace MatchFixer_Web_App.Models.Profile
{
	public class ProfileViewModel
	{
		public string Id { get; set; } = null!;
		public string FirstName { get; set; } = null!;
		public string LastName { get; set; } = null!;
		public string FullName => $"{FirstName} {LastName}";
		public DateTime DateOfBirth { get; set; }
		public string Country { get; set; } = null!;
		public string? TimeZone { get; set; }
		public string Email { get; set; } = null!;
		public string? ProfileImageUrl { get; set; } 
		public DateTime CreatedOn { get; set; }
	}
}
