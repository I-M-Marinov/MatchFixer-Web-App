using MatchFixer_Web_App.Areas.Admin.Hubs;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Bets;
using Microsoft.AspNetCore.SignalR;

namespace MatchFixer.Core.Services
{
	public sealed class AdminInsightsNotifier : IAdminInsightsNotifier
	{
		private readonly IHubContext<AdminInsightsHub> _hub;
		public AdminInsightsNotifier(IHubContext<AdminInsightsHub> hub) => _hub = hub;

		public Task PublishBetMixAsync(BetMixUpdateDto m, CancellationToken ct = default)
			=> _hub.Clients.Group($"evt:{m.EventId}")
				.SendAsync("BetMixUpdated", new
				{
					eventId = m.EventId,
					totalStake = m.TotalStake,
					totalBets = m.TotalBets,
					homePct = m.HomePct,
					drawPct = m.DrawPct,
					awayPct = m.AwayPct
				}, ct);
	}
}
