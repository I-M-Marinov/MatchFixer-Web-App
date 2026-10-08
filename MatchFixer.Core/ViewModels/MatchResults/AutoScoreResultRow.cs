namespace MatchFixer.Core.ViewModels.MatchResults
{
	public class AutoScoreResultRow
	{
		public string HomeTeam { get; set; } = null!;
		public string AwayTeam { get; set; } = null!;
		public string? HomeLogo { get; set; }
		public string? AwayLogo { get; set; }

		public int? HomeScore { get; set; }
		public int? AwayScore { get; set; }

		public bool HasPenalties { get; set; }
		public bool? HomeWonOnPenalties { get; set; }

		// "scored" | "skipped" | "error"
		public string Outcome { get; set; } = "";
		public string? Detail { get; set; }
	}
}
