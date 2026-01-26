using MatchFixer_Web_App.Areas.Admin.ViewModels.Events;
using MatchFixer.Common.Enums;

namespace MatchFixer_Web_App.Areas.Admin.Interfaces
{
	public interface IAdminBetInsightsService
	{
		Task<PaginatedEventList<EventBetStatsRow>>
			GetUpcomingEventBetStatsAsync(
				string? league,
				string? competition, 
				int page,
				int pageSize,
				EventSort sort,
				bool desc,
				CancellationToken ct = default);
	}
}
