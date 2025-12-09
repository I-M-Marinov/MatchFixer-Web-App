using MatchFixer_Web_App.Areas.Admin.ViewModels.Events;

namespace MatchFixer_Web_App.Areas.Admin.Interfaces
{
	public interface IAdminEventsService
	{
		Task<List<AdminEventOverviewDto>> GetFinishedEventsAsync(AdminEventHistoryFilters filters);
	}
}
