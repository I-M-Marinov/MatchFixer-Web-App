namespace MatchFixer.Infrastructure.Entities
{
	public class WorldCupGroupStanding
	{
		public int Id { get; set; }

		public string GroupName { get; set; } = null!;

		public int Rank { get; set; }

		public string TeamName { get; set; } = null!;

		public string TeamBadge { get; set; } = string.Empty;

		public int Played { get; set; }

		public int Wins { get; set; }

		public int Draws { get; set; }

		public int Losses { get; set; }

		public int GoalDifference { get; set; }

		public int Points { get; set; }

		public bool IsQualified { get; set; }

		public DateTime LastUpdated { get; set; }
	}
}
