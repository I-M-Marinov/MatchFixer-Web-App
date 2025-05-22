using System.ComponentModel.DataAnnotations;

namespace MatchFixer.Core.ViewModels.LiveEvents
{
	public class MatchEventFormModel
	{
		[Required] 
		public string HomeTeam { get; set; } = null!;

		[Required]
		public string AwayTeam { get; set; } = null!;

		[Required]
		[DataType(DataType.DateTime)]
		public DateTime MatchDate { get; set; }

		[Range(1.01, 100)]
		public decimal? HomeOdds { get; set; }

		[Range(1.01, 100)]
		public decimal? DrawOdds { get; set; }

		[Range(1.01, 100)]
		public decimal? AwayOdds { get; set; }
	}

}
