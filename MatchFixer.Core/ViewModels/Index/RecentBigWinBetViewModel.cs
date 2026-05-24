using MatchFixer.Common.Enums;

namespace MatchFixer.Core.ViewModels.Index
{
	public class RecentBigWinBetViewModel
	{
		public string HomeTeam { get; set; } = null!;

		public string AwayTeam { get; set; } = null!;
		public string? HomeTeamImageUrl { get; set; }
		public string? AwayTeamImageUrl { get; set; }

		public MatchPick Pick { get; set; }

		public decimal Odds { get; set; }

		public BetStatus Status { get; set; }

		public int? HomeScore { get; set; }

		public int? AwayScore { get; set; }
	}
}
