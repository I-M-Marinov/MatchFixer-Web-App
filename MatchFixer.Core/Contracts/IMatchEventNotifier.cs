using Microsoft.AspNetCore.SignalR;

namespace MatchFixer.Core.Contracts
{
	public interface IMatchEventNotifier
	{
		Task NotifyMatchEventUpdatedAsync(
			Guid matchEventId,
			decimal homeOdds,
			decimal drawOdds,
			decimal awayOdds,
			decimal? effectiveHomeOdds = null,
			decimal? effectiveDrawOdds = null,
			decimal? effectiveAwayOdds = null,
			Guid? activeBoostId = null);

		Task NotifyBoostStartedAsync(
			Guid matchEventId,
			decimal effectiveHomeOdds,
			decimal effectiveDrawOdds,
			decimal effectiveAwayOdds,
			DateTime boostEndUtc,
			decimal maxStake,
			int maxUses);
	}
}
