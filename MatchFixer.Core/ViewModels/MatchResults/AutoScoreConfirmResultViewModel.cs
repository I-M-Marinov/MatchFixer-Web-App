namespace MatchFixer.Core.ViewModels.MatchResults
{
	public class AutoScoreConfirmResultViewModel
	{
		public int Scored { get; set; }
		public int Skipped { get; set; }
		public List<string> Messages { get; set; } = new();
		public List<AutoScoreResultRow> Rows { get; set; } = new();
	}
}
