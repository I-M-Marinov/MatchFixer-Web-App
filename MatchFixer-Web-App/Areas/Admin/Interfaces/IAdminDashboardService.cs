using MatchFixer_Web_App.Areas.Admin.ViewModels;

namespace MatchFixer_Web_App.Areas.Admin.Interfaces
{
	public interface IAdminDashboardService
	{
		Task<AdminDashboardViewModel> GetDashboardAsync();
	}
}
