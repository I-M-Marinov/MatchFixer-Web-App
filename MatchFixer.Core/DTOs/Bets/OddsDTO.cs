
namespace MatchFixer.Core.DTOs.Bets
{
	public class OddsDTO
	{
		public Guid Id { get; set; }

		// Regular odds
		public decimal? HomeOdds { get; set; }
		public decimal? DrawOdds { get; set; }
		public decimal? AwayOdds { get; set; }

		// Effective / boosted odds (null if no active boost)
		public decimal? EffectiveHomeOdds { get; set; }
		public decimal? EffectiveDrawOdds { get; set; }
		public decimal? EffectiveAwayOdds { get; set; }

		public bool HasActiveBoost { get; set; }

	}
}
