using MatchFixer.Core.ViewModels.Profile;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure.Models.Image;
using System.Security.Claims;


namespace MatchFixer.Core.Contracts
{
	public interface IProfileService
	{
		Task<ApplicationUser> GetCurrentUser(string userId);
		Task<ProfileViewModel> GetProfileAsync(string userId);
		Task<(bool Success, string Message)> UpdateProfileAsync(ProfileViewModel model);
		Task<(bool Success, string Message)> UpdateNamesAsync(ProfileViewModel model);
		Task<(bool Success, string ErrorMessage)> UpdateEmailAsync(string userId, string newEmail, string scheme);
		Task<(bool Success, string Message)> ConfirmEmailAsync(string userId, string code);
		Task<ImageResult> UploadProfilePictureAsync(string userId, ImageFileUploadModel imageFileUploadModel);
		Task<ImageResult> RemoveProfilePictureAsync(string userId);
		Task<int?> GetUserRankAsync(string userId);
		Task<bool> DeactivateUserAsync(string userId);
		Task<bool> AnonymizeUserAsync(string userId);
		Task ChangePasswordAsync(ClaimsPrincipal userPrincipal, string currentPassword, string newPassword);
	}
}
