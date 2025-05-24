using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MatchFixer.Core.ViewModels.LiveEvents
{
	public class MatchEventFormModel
	{

		[Required]
		public Guid HomeTeamId { get; set; }

		[Required]
		public Guid AwayTeamId { get; set; }

		public IEnumerable<SelectListItem> Teams { get; set; } = new List<SelectListItem>();

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
