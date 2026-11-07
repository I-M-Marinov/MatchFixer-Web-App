namespace MatchFixer.Core.DTOs.Bets
{
	/// <summary>Result of saving or regenerating a match's extra-market prices (admin).</summary>
	public class MarketOddsResultDto
	{
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;

		/// <summary>Stored prices keyed by selection code ("Over", "Under", "Yes", "No", "1X", "12", "X2").</summary>
		public Dictionary<string, decimal> Odds { get; set; } = new();

		/// <summary>True when the prices were set by hand (Home/Draw/Away edits leave them alone).</summary>
		public bool Manual { get; set; }
	}
}
