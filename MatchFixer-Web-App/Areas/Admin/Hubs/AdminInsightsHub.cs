using Microsoft.AspNetCore.SignalR;
using MatchFixer.Infrastructure.Security;

namespace MatchFixer_Web_App.Areas.Admin.Hubs
{
	[AdminOnly]
	public class AdminInsightsHub : Hub
	{
		public Task JoinEvent(Guid eventId) => Groups.AddToGroupAsync(Context.ConnectionId, $"evt:{eventId}");
		public Task LeaveEvent(Guid eventId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"evt:{eventId}");
	}
}
