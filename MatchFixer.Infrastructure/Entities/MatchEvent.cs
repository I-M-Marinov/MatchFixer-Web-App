
using System.ComponentModel.DataAnnotations;

using static MatchFixer.Common.GeneralConstants.MatchEventValidation;

namespace MatchFixer.Infrastructure.Entities
{
	public class MatchEvent
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		[StringLength(HomeTeamMaxLength, ErrorMessage = HomeTeamCannotExceed100Characters)]
		public string HomeTeam { get; set; } = null!;

		[Required]
		[StringLength(AwayTeamMaxLength, ErrorMessage = AwayTeamCannotExceed100Characters)]
		public string AwayTeam { get; set; } = null!;
		[Required]
		public DateTime MatchDate { get; set; }

		[Range(OddsMinValue, OddsMaxValue)] 
		public decimal? HomeOdds { get; set; }

		[Range(OddsMinValue, OddsMaxValue)]
		public decimal? DrawOdds { get; set; }

		[Range(OddsMinValue, OddsMaxValue)]
		public decimal? AwayOdds { get; set; }

		public ICollection<Bet> Bets { get; set; } = new List<Bet>();
	}
}
