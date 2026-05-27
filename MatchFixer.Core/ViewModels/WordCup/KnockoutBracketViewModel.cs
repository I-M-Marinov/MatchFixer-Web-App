namespace MatchFixer.Core.ViewModels.WordCup
{
	public class KnockoutBracketViewModel
	{
		public List<WorldCupMatchCardViewModel> RoundOf16 { get; set; } = new();

		public List<WorldCupMatchCardViewModel> QuarterFinals { get; set; } = new();

		public List<WorldCupMatchCardViewModel> SemiFinals { get; set; } = new();

		public List<WorldCupMatchCardViewModel> ThirdPlace { get; set; } = new();

		public List<WorldCupMatchCardViewModel> Final { get; set; } = new();
	}
}
