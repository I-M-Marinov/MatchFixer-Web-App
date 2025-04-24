using MatchFixer.Core.Contracts;
using MatchFixer_Web_App.Models.Profile;
using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;

namespace MatchFixer.Core.Services
{
	public class ProfileService : IProfileService
	{
		private readonly UserManager<ApplicationUser> _userManager;

		public ProfileService(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;

		}

		public async Task<ProfileViewModel> GetProfileAsync(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return null;
			}

			return new ProfileViewModel
			{
				Id = user.Id.ToString(),
				FirstName = user.FirstName,
				LastName = user.LastName,
				Email = user.Email!,
				DateOfBirth = user.DateOfBirth,
				Country = user.Country,
				TimeZone = user.TimeZone,
				CreatedOn = user.CreatedOn,
				ProfileImageUrl = user.ProfilePicture?.ImageUrl ?? "/images/user.jpg"
			};
		}

		public async Task<(bool Success, string Message)> UpdateProfileAsync(ProfileViewModel model)
		{
			var user = await _userManager.FindByIdAsync(model.Id);
			

			var changes = new List<string>();

			// Check if user changed the email and if yes update it
			if (user.Email != model.Email)
			{
				user.Email = model.Email;
				changes.Add("Email");
			}
			// Check if user changed the date of birth and if yes update it
			if (user.DateOfBirth != model.DateOfBirth)
			{
				user.DateOfBirth = model.DateOfBirth;
				changes.Add("Date of Birth");
			}
			// Check if user changed the country and if yes update it
			if (user.Country != model.Country)
			{
				user.Country = model.Country;
				changes.Add("Country");
			}

			// If no changes were detected, return a message
			if (changes.Count == 0)
			{
				return (false, "No changes were made to the profile.");
			}

			// Save the changes to the database
			var result = await _userManager.UpdateAsync(user);

			if (!result.Succeeded)
			{
				return (false, "Failed to update profile.");
			}

			// If changes were made, return a message with the updated properties
			string message = "Profile updated successfully. Changed: " + string.Join(", ", changes);
			return (true, message);
		}



	}
}
