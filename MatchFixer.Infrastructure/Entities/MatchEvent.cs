
using System.ComponentModel.DataAnnotations;

namespace MatchFixer.Infrastructure.Entities
{
	public class MatchEvent
	{
		[Key]
		public Guid Id { get; set; }
		[Required]
		public string HomeTeam { get; set; }
		[Required]
		public string AwayTeam { get; set; }
		[Required]
		public DateTime MatchDate { get; set; }

		[Range(1.01, 1000)] 
		public decimal? HomeOdds { get; set; }

		[Range(1.01, 1000)]
		public decimal? DrawOdds { get; set; }

		[Range(1.01, 1000)]
		public decimal? AwayOdds { get; set; }

		public ICollection<Bet> Bets { get; set; } = new List<Bet>();
	}
}
