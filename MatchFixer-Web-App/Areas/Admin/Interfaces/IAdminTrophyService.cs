using MatchFixer.Core.DTOs.UserTrophyContext;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Trophy;

namespace MatchFixer_Web_App.Areas.Admin.Interfaces
{
	public interface IAdminTrophyService
	{
		Task ReevaluateUserTrophiesAsync(Guid userId);
		Task ReevaluateAllUsersAsync();
		Task<List<AdminUserTrophyViewModel>> GetUsersWithTrophiesAsync();
		Task<AdminUserTrophyViewModel?> GetUserWithTrophiesAsync(Guid userId);

	}

}
