
namespace MatchFixer.Infrastructure.Models.TheSportsDBAPI
{
	public class LiveEventApiDto
	{
		public string? idEvent { get; set; }
		public string? idLeague { get; set; }
		public string? strLeague { get; set; }

		public string? strHomeTeam { get; set; }
		public string? strAwayTeam { get; set; }

		public string? intHomeScore { get; set; }
		public string? intAwayScore { get; set; }

		public string? strStatus { get; set; }
	}

}
