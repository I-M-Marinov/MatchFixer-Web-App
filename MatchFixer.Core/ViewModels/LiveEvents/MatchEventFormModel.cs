using MatchFixer.Core.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
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

		public Dictionary<string, List<SelectListItem>> TeamsByLeague { get; set; }
			= new();

		[Required(ErrorMessage = ChooseAValidDateAndTime)]
		[DataType(DataType.DateTime)]
		public DateTime MatchDate { get; set; } = DateTime.Now;

		[Required(ErrorMessage = ChooseValidHomeTeamOdds)]
		[Range(1.01, 100)]
		public decimal? HomeOdds { get; set; }

		[Required(ErrorMessage = ChooseValidDrawOdds)]
		[Range(1.01, 100)]
		public decimal? DrawOdds { get; set; }

		[Required(ErrorMessage = ChooseValidAwayTeamOdds)]
		[Range(1.01, 100)]
		public decimal? AwayOdds { get; set; } 
		public string? CompetitionName { get; set; }
		public bool NoDraw { get; set; } = false;

		/// <summary>
		/// Extra-market prices from the "Markets preview" panel, keyed by selection code
		/// ("Over", "Under", "Yes", "No", "1X", "12", "X2"). Empty → generated from the 1X2 odds.
		/// </summary>
		public Dictionary<string, decimal?> MarketOdds { get; set; } = new();

		/// <summary>True when the admin changed any preview price by hand before saving.</summary>
		public bool MarketOddsEdited { get; set; }
		public List<SelectListItem> AvailableCompetitions { get; set; } = new();

		public List<LiveEventViewModel> CurrentEvents { get; set; } = new();

		public List<ApiLeagueSelectViewModel> ApiLeagues { get; set; } = new();
	}


}
