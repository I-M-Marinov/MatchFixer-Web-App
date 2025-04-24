using MatchFixer_Web_App.Models.Profile;


namespace MatchFixer.Core.Contracts
{
	public interface IProfileService
	{
		Task<ProfileViewModel> GetProfileAsync(string userId);
		Task<(bool Success, string Message)> UpdateProfileAsync(ProfileViewModel model);
	}
}
