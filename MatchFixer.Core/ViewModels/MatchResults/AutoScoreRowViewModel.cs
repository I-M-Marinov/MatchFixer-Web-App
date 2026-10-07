using MatchFixer.Common.Enums;

namespace MatchFixer.Core.ViewModels.MatchResults
{
	public class AutoScoreRowViewModel
	{
		public Guid MatchId { get; set; }
		public int ApiFixtureId { get; set; }

		public string HomeTeam { get; set; } = null!;
		public string AwayTeam { get; set; } = null!;

		public string? HomeLogo { get; set; }
		public string? AwayLogo { get; set; }

		public DateTime? KickoffUtc { get; set; }
		public string DisplayTime { get; set; } = null!;

		public AutoScoreState State { get; set; }
		public string StatusShort { get; set; } = "-";   // raw API code (FT, 2H, NS...)
		public string StatusLabel { get; set; } = "";    // human-readable label
		public int? Elapsed { get; set; }

		public int? HomeScore { get; set; }
		public int? AwayScore { get; set; }
		public bool? HomeWonOnPenalties { get; set; }
		public bool HasPenalties { get; set; }

		public bool CanScore =>
			State == AutoScoreState.Finished && HomeScore.HasValue && AwayScore.HasValue;
	}
}
