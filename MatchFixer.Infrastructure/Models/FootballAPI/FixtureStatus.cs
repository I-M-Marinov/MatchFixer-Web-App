namespace MatchFixer.Infrastructure.Models.FootballAPI
{
	public class FixtureStatus
	{
		// API-Football fixture status codes, e.g. NS, 1H, HT, 2H, ET, P, FT, AET, PEN, PST, CANC, ABD, SUSP, INT, LIVE
		public string? Long { get; set; }
		public string? Short { get; set; }
		public int? Elapsed { get; set; }
	}
}
