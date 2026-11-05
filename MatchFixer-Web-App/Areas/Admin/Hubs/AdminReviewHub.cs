using MatchFixer.Infrastructure.Security;
using Microsoft.AspNetCore.SignalR;

namespace MatchFixer_Web_App.Areas.Admin.Hubs
{
	[AdminOnly]
	public class AdminReviewHub : Hub
	{
	}
}
