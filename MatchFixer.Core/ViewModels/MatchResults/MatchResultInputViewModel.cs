using System.ComponentModel.DataAnnotations;

namespace MatchFixer.Core.ViewModels.MatchResults
{
	public class MatchResultInputViewModel
	{
		public Guid MatchId { get; set; }

		[Required] 
		public string HomeTeam { get; set; } = null!;

		[Required]
		public string AwayTeam { get; set; } = null!;

		public DateTime MatchDate { get; set; }

		public string DisplayTime { get; set; } = null!;

		[Required]
		[Range(0, 20, ErrorMessage = "Score must be between 0 and 20.")]
		public int HomeScore { get; set; }

		[Required]
		[Range(0, 20, ErrorMessage = "Score must be between 0 and 20.")]
		public int AwayScore { get; set; }

		public string? Notes { get; set; }
	}
}
