
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
		public bool NoDraw { get; set; }

		// Extra markets keyed by selection code ("Over", "Under", "Yes", "No", "1X", "12", "X2").
		// Codes are unique across markets, so the selection alone identifies the market. Not boosted.
		public Dictionary<string, decimal> MarketOdds { get; set; } = new();
	}
}
