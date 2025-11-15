using MatchFixer.Core.ViewModels.EventsResults;

namespace MatchFixer.Core.Contracts
{
	public interface IEventsResultsService
	{
		Task<IReadOnlyList<EventsResults>> GetLatestAsync(int count = 10);

	}
}
