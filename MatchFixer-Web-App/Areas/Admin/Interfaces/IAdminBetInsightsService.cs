using MatchFixer_Web_App.Areas.Admin.ViewModels.Events;

namespace MatchFixer_Web_App.Areas.Admin.Interfaces
{
	public interface IAdminBetInsightsService
	{
		Task<PaginatedEventList<EventBetStatsRow>> GetUpcomingEventBetStatsAsync(
				string? league,
				int page,
				int pageSize,
				CancellationToken ct);
	}
}
