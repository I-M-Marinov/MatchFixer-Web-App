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
			decimal? boostValue,
			DateTime startUtc,
			DateTime boostEndUtc,
			decimal maxStake,
			int maxUses);

		Task NotifyMatchPostponedAsync(
			Guid matchEventId,
			DateTime? newKickoffUtc = null);


		Task BroadcastBoostStartedAsync(BoostRealtimeMessage msg, CancellationToken ct = default);

		public sealed class BoostRealtimeMessage
		{
			public Guid MatchEventId { get; init; }
			public decimal EffectiveHomeOdds { get; init; }
			public decimal EffectiveDrawOdds { get; init; }
			public decimal EffectiveAwayOdds { get; init; }
			public decimal? BoostValue { get; init; }
			public DateTime StartUtc { get; init; }
			public DateTime EndUtc { get; init; }
			public decimal? MaxStake { get; init; }
			public int? MaxUses { get; init; }
			public string? Label { get; init; }
			public string? HomeName { get; init; }
			public string? AwayName { get; init; }
		}
	}
}
