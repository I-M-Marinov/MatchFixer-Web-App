
namespace MatchFixer.Infrastructure.Models.TheSportsDBAPI
{
	public class LiveEventApiDto
	{
		public string? strHomeTeam { get; set; }
		public string? strAwayTeam { get; set; }

		public string? intHomeScore { get; set; }
		public string? intAwayScore { get; set; }

		public string? strStatus { get; set; } 
	}
}
