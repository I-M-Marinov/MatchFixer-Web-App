
namespace MatchFixer.Core.DTOs.Bets
{
	public class OddsDTO
	{ 
		public Guid Id { get; set; }
		public decimal? HomeOdds { get; set; }
		public decimal? DrawOdds { get; set; }
		public decimal? AwayOdds { get; set; }
	}
}
