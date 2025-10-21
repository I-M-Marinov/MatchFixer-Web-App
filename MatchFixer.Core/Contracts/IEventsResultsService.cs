using MatchFixer.Core.ViewModels.EventsResults;

namespace MatchFixer_Web_App.Areas.Admin.Interfaces
{
	public interface IEventsResultsService
	{
		Task<IReadOnlyList<EventsResults>> GetLatestAsync(int count = 10);

	}
}
