using MatchFixer.Common.Enums;
using MatchFixer.Infrastructure.Models.TheSportsDBAPI;

namespace MatchFixer.Infrastructure.Contracts
{
	public interface ILiveMatchQueryService
	{
		Task<IReadOnlyList<LiveTeamMatchInfo>> GetLiveMatchesForLeagueAsync(
			InternalLeague league,
			CancellationToken ct = default);
	}
}
