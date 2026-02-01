namespace MatchFixer.Core.ViewModels.LeagueTables
{
	public class LeagueTableRowViewModel
	{
		public int Rank { get; set; }
		public string Team { get; set; }
		public string Badge { get; set; }
		public int Played { get; set; }
		public int Wins { get; set; }
		public int Draws { get; set; }
		public int Losses { get; set; }
		public int GoalDiff { get; set; }
		public int Points { get; set; }
	}

}
