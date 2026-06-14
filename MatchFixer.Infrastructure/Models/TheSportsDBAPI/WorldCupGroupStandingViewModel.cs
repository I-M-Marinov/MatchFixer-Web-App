
namespace MatchFixer.Infrastructure.Models.TheSportsDBAPI
{
	public class WorldCupGroupStandingViewModel
	{
		public string GroupName { get; set; } = null!;

		public List<WorldCupStandingTeamViewModel> Teams { get; set; }
			= new();
	}
}
