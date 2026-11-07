using MatchFixer.Common.Enums;

namespace MatchFixer.Core.DTOs.Bets
{
	public class SingleBetDto
	{
		public Guid MatchId { get; set; }
		public string HomeTeam { get; set; }
		public string AwayTeam { get; set; }
		public string SelectedOption { get; set; } // "Home", "Draw", "Away" (or a display label for extra markets)
		public BettingMarket Market { get; set; } = BettingMarket.MatchResult;
		public string? Selection { get; set; } // "Over", "Yes", "1X"... for extra markets
		public decimal? Line { get; set; }
		public decimal Odds { get; set; }
		public string Outcome { get; set; } = string.Empty;
		public string? Status { get; set; }
	}
}
