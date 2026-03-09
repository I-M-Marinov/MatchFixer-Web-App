
using System.ComponentModel.DataAnnotations;


namespace MatchFixer.Core.ViewModels.MatchResults
{
	public class ScoreMatchInputModel
	{
		public Guid MatchId { get; set; }

		[Required]
		[Range(0, 20, ErrorMessage = "Score must be between 0 and 20.")]
		public int HomeScore { get; set; }

		[Required]
		[Range(0, 20, ErrorMessage = "Score must be between 0 and 20.")]
		public int AwayScore { get; set; }

		public string? Notes { get; set; }
	}
}
