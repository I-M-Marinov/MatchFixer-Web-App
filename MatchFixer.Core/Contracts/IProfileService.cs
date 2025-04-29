using MatchFixer.Core.ViewModels.Profile;
using MatchFixer.Infrastructure.Models.Image;


namespace MatchFixer.Core.Contracts
{
	public interface IProfileService
	{
		Task<ProfileViewModel> GetProfileAsync(string userId);
		Task<(bool Success, string Message)> UpdateProfileAsync(ProfileViewModel model);
		Task<(bool Success, string Message)> UpdateNamesAsync(ProfileViewModel model);
		Task<(bool Success, string ErrorMessage)> UpdateEmailAsync(string userId, string newEmail, string scheme);
		Task<(bool Success, string Message)> ConfirmEmailAsync(string userId, string code);
		Task<ImageResult> UploadProfilePictureAsync(string userId, ImageFileUploadModel imageFileUploadModel);
		Task<ImageResult> RemoveProfilePictureAsync(string userId);

	}
}
