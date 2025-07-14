using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using MatchFixer.Core.ValidationAttributes;

using static MatchFixer.Common.ValidationConstants.MatchEventFormValidations;

namespace MatchFixer.Core.ViewModels.LiveEvents
{
	[DifferentTeams]
	public class MatchEventFormModel
	{

		[Required(ErrorMessage = ChooseAValidTeam)]
		public Guid HomeTeamId { get; set; }

		[Required(ErrorMessage = ChooseAValidTeam)]
		public Guid AwayTeamId { get; set; }

		public Dictionary<string, List<SelectListItem>> TeamsByLeague { get; set; } = new Dictionary<string, List<SelectListItem>>();

		[Required(ErrorMessage = ChooseAValidDateAndTime)]
		[DataType(DataType.DateTime)]
		public DateTime MatchDate { get; set; } = DateTime.UtcNow;

		[Required(ErrorMessage = ChooseValidHomeTeamOdds)]
		[Range(1.01, 100)]
		public decimal? HomeOdds { get; set; }

		[Required(ErrorMessage = ChooseValidDrawOdds)]
		[Range(1.01, 100)]
		public decimal? DrawOdds { get; set; }

		[Required(ErrorMessage = ChooseValidAwayTeamOdds)]
		[Range(1.01, 100)]
		public decimal? AwayOdds { get; set; }

		public List<LiveEventViewModel> CurrentEvents { get; set; } = new();

	}

}
