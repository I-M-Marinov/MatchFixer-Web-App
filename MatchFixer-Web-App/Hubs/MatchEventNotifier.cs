using MatchFixer.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using static MatchFixer.Core.Contracts.IMatchEventNotifier;

namespace MatchFixer_Web_App.Hubs
{

	[AllowAnonymous]
	public class MatchEventNotifier : IMatchEventNotifier
	{
		private readonly IHubContext<MatchEventHub> _hubContext;


		public MatchEventNotifier(IHubContext<MatchEventHub> hubContext)
		{
			_hubContext = hubContext;
		}

		public Task NotifyMatchEventUpdatedAsync(
			Guid matchEventId,
			decimal homeOdds,
			decimal drawOdds,
			decimal awayOdds,
			decimal? effectiveHomeOdds = null,
			decimal? effectiveDrawOdds = null,
			decimal? effectiveAwayOdds = null,
			Guid? activeBoostId = null)
		{
			var payload = new
			{
				matchEventId,
				homeOdds,
				drawOdds,
				awayOdds,
				effectiveHomeOdds = effectiveHomeOdds ?? homeOdds,
				effectiveDrawOdds = effectiveDrawOdds ?? drawOdds,
				effectiveAwayOdds = effectiveAwayOdds ?? awayOdds,
				activeBoostId
			};

			return Task.WhenAll(
				// Match-specific listeners (live events, match cards)
				_hubContext.Clients
					.Group(matchEventId.ToString())
					.SendAsync("MatchEventUpdated", payload),

				// Boost carousel listeners 
				_hubContext.Clients
					.Group(MatchEventHub.BoostWatchers)
					.SendAsync("MatchEventUpdated", payload)
			);
		}

		public Task NotifyBoostStartedAsync(
			Guid matchEventId,
			decimal effectiveHomeOdds,
			decimal effectiveDrawOdds,
			decimal effectiveAwayOdds,
			decimal? boostValue,
			DateTime startUtc,
			DateTime boostEndUtc,
			decimal maxStake,
			int maxUses)
		{

			var startUtcFixed = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);
			var endUtcFixed = DateTime.SpecifyKind(boostEndUtc, DateTimeKind.Utc);
			

			return _hubContext.Clients
				.Group(matchEventId.ToString())
				.SendAsync("BoostStarted", new
				{
					matchEventId = matchEventId.ToString(),
					startUtc = startUtcFixed,
					endUtc = endUtcFixed,
					boostValue,
					effectiveHomeOdds,
					effectiveDrawOdds,
					effectiveAwayOdds,
					maxStake,
					maxUses
				});
		}

		public async Task BroadcastBoostStartedAsync(BoostRealtimeMessage msg, CancellationToken ct = default)
		{
			var dto = new MatchEventHub.BoostDto
			{
				MatchEventId = msg.MatchEventId,
				EffectiveHomeOdds = msg.EffectiveHomeOdds,
				EffectiveDrawOdds = msg.EffectiveDrawOdds,
				EffectiveAwayOdds = msg.EffectiveAwayOdds,
				BoostAmount = msg.BoostValue,
				StartUtc = msg.StartUtc,
				EndUtc = msg.EndUtc,
				MaxStake = msg.MaxStake,
				MaxUses = msg.MaxUses,
				Label = msg.Label,
				HomeName = msg.HomeName,
				AwayName = msg.AwayName
			};

			await Task.WhenAll(
				_hubContext.Clients.Group(MatchEventHub.BoostWatchers)
					.SendAsync("BoostStarted", dto, ct),
				_hubContext.Clients.Group(msg.MatchEventId.ToString())
					.SendAsync("BoostStarted", dto, ct)
			);
		}

	}

}
