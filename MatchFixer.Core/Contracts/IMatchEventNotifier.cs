using Microsoft.AspNetCore.SignalR;

namespace MatchFixer.Core.Contracts
{
	public interface IMatchEventNotifier
	{
		Task NotifyMatchEventUpdatedAsync(Guid matchEventId, decimal homeOdds, decimal drawOdds, decimal awayOdds);
	}
}
