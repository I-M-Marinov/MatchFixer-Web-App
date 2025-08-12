using Microsoft.AspNetCore.SignalR;

namespace MatchFixer_Web_App.Hubs
{
	public class MatchEventHub: Hub
	{
		public async Task SubscribeToEvent(Guid matchEventId)
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, matchEventId.ToString());
		}
	}
}
