namespace MatchFixer.Infrastructure.Models.FootballAPI
{
	/// <summary>
	/// Flattened result for a single API fixture, used by the auto-scoring flow.
	/// </summary>
	public class FixtureResultDto
	{
		public int ApiFixtureId { get; set; }
		public string? StatusShort { get; set; }
		public string? StatusLong { get; set; }
		public int? Elapsed { get; set; }
		public int? HomeGoals { get; set; }
		public int? AwayGoals { get; set; }
		// From teams.home.winner: true = home won, false = away won, null = draw / undecided
		public bool? HomeIsWinner { get; set; }
		public bool HasPenalties { get; set; }
	}
}
