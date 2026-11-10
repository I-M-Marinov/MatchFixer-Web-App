using MatchFixer.Common.Enums;
using MatchFixer.Core.Helpers;

namespace MatchFixer.Core.ViewModels.Index
{
	public class RecentBigWinBetViewModel
	{
		public string HomeTeam { get; set; } = null!;

		public string AwayTeam { get; set; } = null!;
		public string? HomeTeamImageUrl { get; set; }
		public string? AwayTeamImageUrl { get; set; }

		public MatchPick Pick { get; set; }

		public BettingMarket Market { get; set; } = BettingMarket.MatchResult;
		public string? Selection { get; set; }
		public decimal? Line { get; set; }

		/// <summary>Display label for the pick ("Home", "Over 2.5", "BTTS Yes"...).</summary>
		public string PickLabel => BetMarketHelper.DisplayPick(Market, Pick, Selection, Line);

		public decimal Odds { get; set; }

		public BetStatus Status { get; set; }

		public int? HomeScore { get; set; }

		public int? AwayScore { get; set; }

		public bool? HomeWonOnPenalties { get; set; }
	}
}
