using MatchFixer_Web_App.Areas.Admin.ViewModels.Events;

namespace MatchFixer_Web_App.Areas.Admin.Interfaces
{
	public interface IAdminEventsService
	{
		Task<PaginatedEventList<AdminEventOverviewDto>> GetFinishedEventsAsync(AdminEventHistoryFilters filters);
		Task<List<AdminTeamBettingStatsDto>> GetTeamBettingStatsAsync(AdminEventHistoryFilters filters);

	}
}
