namespace MatchFixer.Core.Contracts
{
	public interface IBoostQueryService
	{
		Task<IReadOnlyList<ActiveBoost>> GetActiveBoostsAsync(IEnumerable<Guid> matchEventIds);

		public class ActiveBoost
		{
			public Guid MatchEventId { get; init; }
			public decimal EffectiveHomeOdds { get; init; }
			public decimal EffectiveDrawOdds { get; init; }
			public decimal EffectiveAwayOdds { get; init; }
			public DateTime StartUtc { get; init; }
			public DateTime EndUtc { get; init; }
			public decimal? MaxStake { get; init; }
			public int? MaxUses { get; init; }
			public string? Label { get; init; }
			public string? HomeTeamName { get; init; }
			public string? AwayTeamName { get; init; }
		}
	}
}
