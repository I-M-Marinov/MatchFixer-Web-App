

namespace MatchFixer.Core.ViewModels.DTO
{
	public class ActiveBoost
	{
		public Guid MatchEventId { get; init; }
		public decimal EffectiveHomeOdds { get; init; }
		public decimal EffectiveDrawOdds { get; init; }
		public decimal EffectiveAwayOdds { get; init; }
		public DateTime StartUtc { get; init; }
		public DateTime EndUtc { get; init; }
		public decimal? MaxStake { get; init; }   // ← nullable to match OddsBoost
		public int? MaxUses { get; init; }        // ← nullable to match OddsBoost
		public string? Label { get; init; }       // we’ll use OddsBoost.Note here
		public string? HomeTeamName { get; init; }
		public string? AwayTeamName { get; init; }
	}
}
