using MatchFixer_Web_App.Areas.Admin.ViewModels;

namespace MatchFixer_Web_App.Areas.Admin.Services
{
	public interface IAdminDashboardService
	{
		Task<AdminDashboardViewModel> GetDashboardAsync();
	}
}
