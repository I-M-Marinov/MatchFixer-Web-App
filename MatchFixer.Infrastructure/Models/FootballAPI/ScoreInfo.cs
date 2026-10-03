namespace MatchFixer.Infrastructure.Models.FootballAPI
{
	public class ScoreInfo
	{
		public Goals? Halftime { get; set; }
		public Goals? Fulltime { get; set; }
		public Goals? Extratime { get; set; }
		public Goals? Penalty { get; set; }
	}
}
