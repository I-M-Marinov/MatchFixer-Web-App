namespace MatchFixer.Core.ViewModels.WordCup
{
	public class WorldCupStageViewModel
	{
		public string StageName { get; set; } = null!;

		public List<WorldCupMatchCardViewModel> Matches { get; set; } = new();
	}
}
