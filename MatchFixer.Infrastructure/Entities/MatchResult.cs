using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		[StringLength(100, ErrorMessage = "League name cannot exceed 100 characters.")]
		public string LeagueName { get; set; }

		[Required]
		[Range(1900, 2100, ErrorMessage = "Season must be a valid year.")]
		public int Season { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "Home team name cannot exceed 100 characters.")]
		public string HomeTeam { get; set; }

		[Url]
		[StringLength(300, ErrorMessage = "Home team logo URL cannot exceed 300 characters.")]
		public string HomeTeamLogo { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "Away team name cannot exceed 100 characters.")]
		public string AwayTeam { get; set; }

		[Url]
		[StringLength(300, ErrorMessage = "Away team logo URL cannot exceed 300 characters.")]
		public string AwayTeamLogo { get; set; }

		[Range(0, 99, ErrorMessage = "Score must be between 0 and 99.")]
		public int? HomeScore { get; set; }

		[Range(0, 99, ErrorMessage = "Score must be between 0 and 99.")]
		public int? AwayScore { get; set; }
	}
}
