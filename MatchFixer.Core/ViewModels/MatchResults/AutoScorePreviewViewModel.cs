namespace MatchFixer.Core.ViewModels.MatchResults
{
	public class AutoScorePreviewViewModel
	{
		public List<AutoScoreRowViewModel> Rows { get; set; } = new();

		public int FinishedCount { get; set; }
		public int InProgressCount { get; set; }
		public int OtherCount { get; set; }

		public string? Message { get; set; }
	}
}
