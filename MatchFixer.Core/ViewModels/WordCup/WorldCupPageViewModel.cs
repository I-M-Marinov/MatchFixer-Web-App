using MatchFixer.Infrastructure.Models.TheSportsDBAPI;

namespace MatchFixer.Core.ViewModels.WordCup
{
	public class WorldCupPageViewModel
	{
		public List<WorldCupStageViewModel> Stages { get; set; } = new();

		public KnockoutBracketViewModel Bracket { get; set; } = null!;

		public List<WorldCupGroupStandingViewModel> GroupStandings
		{ get; set; } = new();


	}
}

