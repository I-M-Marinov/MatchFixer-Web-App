using System.ComponentModel.DataAnnotations;

namespace MatchFixer.Core.ViewModels.MatchResults
{
	public class MatchResultInputViewModel
	{
		public Guid MatchId { get; set; }

		public string HomeTeam { get; set; }
		public string AwayTeam { get; set; }

		public DateTime MatchDate { get; set; }

		[Required]
		[Range(0, 20)]
		public int HomeScore { get; set; }

		[Required]
		[Range(0, 20)]
		public int AwayScore { get; set; }

		public string? Notes { get; set; }
	}
}
