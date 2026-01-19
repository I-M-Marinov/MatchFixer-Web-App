using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.DTO;
using Microsoft.AspNetCore.SignalR;

namespace MatchFixer_Web_App.Hubs
{
	public class MatchEventHub : Hub
	{
		private readonly IBoostQueryService _boosts;

		public MatchEventHub(IBoostQueryService boosts)
		{
			_boosts = boosts;
		}

		public const string BoostWatchers = "boost-watchers";

		
		public override async Task OnConnectedAsync()
		{
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception? ex)
		{
			await base.OnDisconnectedAsync(ex);
		}

		public Task SubscribeToEvent(Guid matchEventId) =>
			Groups.AddToGroupAsync(Context.ConnectionId, matchEventId.ToString());

		public Task JoinMatch(string matchEventId)
		{
			if (!Guid.TryParse(matchEventId, out var id))
			{
				return Task.CompletedTask;
			}
			return Groups.AddToGroupAsync(Context.ConnectionId, id.ToString());
		}
		public Task JoinBoostChannel() => Groups.AddToGroupAsync(Context.ConnectionId, BoostWatchers);

		public Task LeaveMatch(string matchEventId)
		{
			if (!Guid.TryParse(matchEventId, out var id))
			{
				return Task.CompletedTask;
			}
			return Groups.RemoveFromGroupAsync(Context.ConnectionId, id.ToString());
		}

		public async Task<IEnumerable<BoostDto>> GetActiveBoosts(List<string>? matchEventIds)
		{
			try
			{

				var ids = new List<Guid>();
				

				IReadOnlyList<ActiveBoost> active =
					ids.Count == 0
						? await _boosts.GetAllActiveBoostsAsync(limit: 20)  
						: await _boosts.GetActiveBoostsAsync(ids);


				return active.Select(a => new BoostDto
				{
					MatchEventId = a.MatchEventId,
					BoostAmount = a.BoostAmount,
					EffectiveHomeOdds = a.EffectiveHomeOdds,
					EffectiveDrawOdds = a.EffectiveDrawOdds,
					EffectiveAwayOdds = a.EffectiveAwayOdds,
					StartUtc = DateTime.SpecifyKind(a.StartUtc, DateTimeKind.Utc),
					EndUtc = DateTime.SpecifyKind(a.EndUtc, DateTimeKind.Utc),
					MaxStake = a.MaxStake,
					MaxUses = a.MaxUses,
					Label = a.Label,
					HomeName = a.HomeTeamName,
					AwayName = a.AwayTeamName,
					HomeTeamLogo = a.HomeTeamLogo,
					AwayTeamLogo = a.AwayTeamLogo
				});
			}
			catch (Exception ex)
			{
				return Array.Empty<BoostDto>();
			}
		}
			
		public Task<string> Ping() => Task.FromResult($"ok {DateTime.UtcNow:o}");

		public sealed class BoostDto
		{
			public Guid MatchEventId { get; set; }
			public decimal? BoostAmount { get; set; }
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
			public string? HomeTeamLogo { get; init; }
			public string? AwayTeamLogo { get; init; }
		}
	}
}
