using MatchFixer.Core.ViewModels.EventsResults;

namespace MatchFixer.Core.Contracts
{
	public interface IEventsResultsService
	{
		Task<PagedDayEventResult<EventsResults>> GetByDayAsync(int offset = 0);
	}
}
