using System.ComponentModel.DataAnnotations;

using static MatchFixer.Common.GeneralConstants.MatchResultValidation;

namespace MatchFixer.Infrastructure.Entities
{
	public class MatchResult
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public int ApiFixtureId { get; set; }

		[Required]
		public DateTime Date { get; set; }

		[Required]
		[StringLength(LeagueNameMaxLength, ErrorMessage = LeagueNameCannotExceed100Characters)]
		public string LeagueName { get; set; } = null!;

		[Required]
		[Range(YearMinValue, YearMaxValue, ErrorMessage = YearMustBeValid)]
		public int Season { get; set; }

		[Required]
		[StringLength(HomeTeamMaxLength, ErrorMessage = HomeTeamCannotExceed100Characters)]
		public string HomeTeam { get; set; } = null!;

		[Url]
		[StringLength(HomeTeamLogoUrlMaxLength, ErrorMessage = HomeTeamLogoUrlCannotExceed300Characters)]
		public string HomeTeamLogo { get; set; } = null!;

		[Required]
		[StringLength(AwayTeamMaxLength, ErrorMessage = AwayTeamCannotExceed100Characters)]
		public string AwayTeam { get; set; } = null!;

		[Url]
		[StringLength(AwayTeamLogoUrlMaxLength, ErrorMessage = AwayTeamLogoUrlCannotExceed300Characters)]
		public string AwayTeamLogo { get; set; } = null!;

		[Range(ScoreMinValue, ScoreMaxValue, ErrorMessage = ScoreMustBeValid)]
		public int? HomeScore { get; set; }

		[Range(ScoreMinValue, ScoreMaxValue, ErrorMessage = ScoreMustBeValid)]
		public int? AwayScore { get; set; }
	}
}
