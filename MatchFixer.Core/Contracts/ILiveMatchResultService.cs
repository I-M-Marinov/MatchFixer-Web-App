using MatchFixer.Core.ViewModels.MatchResults;

namespace MatchFixer.Core.Contracts
{
	public interface ILiveMatchResultService
	{
		Task<bool> AddMatchResultAsync(Guid matchEventId, int homeScore, int awayScore, string? notes = null);
		Task<List<MatchResultInputViewModel>> GetUnresolvedMatchResultsAsync();
	}

}
