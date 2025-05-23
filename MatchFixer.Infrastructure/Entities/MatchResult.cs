using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

		public Guid HomeTeamId { get; set; }
		public Guid AwayTeamId { get; set; }

		[ForeignKey(nameof(HomeTeamId))]
		public Team HomeTeam { get; set; }

		[ForeignKey(nameof(AwayTeamId))]
		public Team AwayTeam { get; set; }

		[Range(ScoreMinValue, ScoreMaxValue, ErrorMessage = ScoreMustBeValid)]
		public int? HomeScore { get; set; }

		[Range(ScoreMinValue, ScoreMaxValue, ErrorMessage = ScoreMustBeValid)]
		public int? AwayScore { get; set; }
	}
}
