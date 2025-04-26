using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MatchFixer.Core.ViewModels.Profile
{
	public class ProfileViewModel
	{
		public string Id { get; set; } = null!;
		[Required(ErrorMessage = "First name is required.")]
		public string FirstName { get; set; } = null!;
		[Required(ErrorMessage = "Last name is required.")]
		public string LastName { get; set; } = null!;
		public string FullName => $"{FirstName} {LastName}";
		public DateTime DateOfBirth { get; set; }
		public string Country { get; set; } = null!;
		[Required(ErrorMessage = "Time Zone is required. Please select the time zone.")]
		public string TimeZone { get; set; } = null!;

		[Required(ErrorMessage = "Email address is required.")]
		public string Email { get; set; } = null!;
		public string? ProfileImageUrl { get; set; }
		public DateTime CreatedOn { get; set; }
		public List<SelectListItem> CountryOptions { get; set; } = new();


	}
}
