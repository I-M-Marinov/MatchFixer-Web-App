using MatchFixer.Common.Enums;

namespace MatchFixer.Infrastructure.Models.TheSportsDBAPI
{
	public class LeagueTablePageViewModel
	{
		public InternalLeague SelectedLeague { get; set; }
		public List<LeagueTableRowApiDto> Table { get; set; } = new();
	}

}
