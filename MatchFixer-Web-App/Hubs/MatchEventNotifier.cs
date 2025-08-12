using Microsoft.AspNetCore.SignalR;
using MatchFixer.Core.Contracts;

namespace MatchFixer_Web_App.Hubs
{
	public class MatchEventNotifier : IMatchEventNotifier
	{
		private readonly IHubContext<MatchEventHub> _hubContext;

		public MatchEventNotifier(IHubContext<MatchEventHub> hubContext)
		{
			_hubContext = hubContext;
		}

		public Task NotifyMatchEventUpdatedAsync(Guid matchEventId, decimal homeOdds, decimal drawOdds, decimal awayOdds)
		{
			return _hubContext.Clients
				.Group(matchEventId.ToString())
				.SendAsync("MatchEventUpdated", new
				{
					matchEventId,
					homeOdds,
					drawOdds,
					awayOdds
				});
		}
	}

}
