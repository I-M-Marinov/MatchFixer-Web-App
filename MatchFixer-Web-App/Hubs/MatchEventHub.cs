using MatchFixer.Core.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace MatchFixer_Web_App.Hubs
{
	public class MatchEventHub: Hub
	{
		public async Task SubscribeToEvent(Guid matchEventId)
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, matchEventId.ToString());
		}

		private readonly IBoostQueryService _boosts;


		public Task JoinMatch(Guid matchEventId) =>
			Groups.AddToGroupAsync(Context.ConnectionId, matchEventId.ToString());

		public Task LeaveMatch(Guid matchEventId) =>
			Groups.RemoveFromGroupAsync(Context.ConnectionId, matchEventId.ToString());

		public async Task<IEnumerable<BoostDto>> GetActiveBoosts(IEnumerable<Guid> matchEventIds)
		{
			var active = await _boosts.GetActiveBoostsAsync(matchEventIds);
			return active.Select(a => new BoostDto
			{
				MatchEventId = a.MatchEventId,
				EffectiveHomeOdds = a.EffectiveHomeOdds,
				EffectiveDrawOdds = a.EffectiveDrawOdds,
				EffectiveAwayOdds = a.EffectiveAwayOdds,
				StartUtc = a.StartUtc,
				EndUtc = a.EndUtc,
				MaxStake = a.MaxStake,
				MaxUses = a.MaxUses,
				Label = a.Label,
				HomeName = a.HomeTeamName,
				AwayName = a.AwayTeamName
			});
		}

		public sealed class BoostDto
		{
			public Guid MatchEventId { get; set; }
			public decimal EffectiveHomeOdds { get; set; }
			public decimal EffectiveDrawOdds { get; set; }
			public decimal EffectiveAwayOdds { get; set; }
			public DateTime StartUtc { get; set; }
			public DateTime EndUtc { get; set; }
			public decimal? MaxStake { get; set; }
			public int? MaxUses { get; set; }
			public string? Label { get; set; }
			public string? HomeName { get; set; }
			public string? AwayName { get; set; }
		}

	}
}
