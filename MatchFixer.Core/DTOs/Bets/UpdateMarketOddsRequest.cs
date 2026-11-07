namespace MatchFixer.Core.DTOs.Bets
{
	public class UpdateMarketOddsRequest
	{
		public Guid MatchEventId { get; set; }

		/// <summary>Prices keyed by selection code ("Over", "Under", "Yes", "No", "1X", "12", "X2").</summary>
		public Dictionary<string, decimal> Odds { get; set; } = new();
	}

	public class RegenerateMarketOddsRequest
	{
		public Guid MatchEventId { get; set; }
	}

	public class SetExtraMarketsRequest
	{
		public Guid MatchEventId { get; set; }
		public bool Enabled { get; set; }

		/// <summary>"OverUnderGoals", "BothTeamsToScore", "DoubleChance", or empty for all extra markets of the match.</summary>
		public string? Market { get; set; }
	}
}
