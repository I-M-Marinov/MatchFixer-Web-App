using MatchFixer_Web_App.Areas.Admin.ViewModels;
using MatchFixer_Web_App.Areas.Admin.ViewModels.MatchEvents;

namespace MatchFixer_Web_App.Areas.Admin.Interfaces
{
	public interface IAdminDashboardService
	{
		Task<AdminDashboardViewModel> GetDashboardAsync();
		Task<AdminEventsSummaryViewModel> BuildEventsSummaryAsync(int takeRecent);
	}
}
