namespace MatchFixer.Infrastructure.Models.TheSportsDBAPI
{
	public class WorldCupStandingTeamViewModel
	{
		public string TeamName { get; set; } = null!;

		public string TeamLogo { get; set; } = null!;

		public int Played { get; set; }

		public int Wins { get; set; }

		public int Draws { get; set; }

		public int Losses { get; set; }

		public int GoalsFor { get; set; }

		public int GoalsAgainst { get; set; }

		public int GoalDifference { get; set; }

		public int Points { get; set; }

		public bool IsQualified { get; set; }
	}
}
