namespace MatchFixer.Infrastructure.Models.TheSportsDBAPI
{
	public class LiveTeamMatchInfo
	{
		public string TeamName { get; set; } = null!;
		public string Opponent { get; set; } = null!;
		public int GoalsFor { get; set; }
		public int GoalsAgainst { get; set; }
	}
}
