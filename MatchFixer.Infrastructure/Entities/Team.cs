using System.ComponentModel.DataAnnotations;

using static MatchFixer.Common.GeneralConstants.MatchResultValidation;


namespace MatchFixer.Infrastructure.Entities
{
	public class Team
	{
		[Key]
		public Guid Id { get; set; }

		public int? TeamId { get; set; }

		[Required]
		[StringLength(100)]
		public string Name { get; set; } = null!;

		[Required] 
		[Url]
		[StringLength(250)]
		public string LogoUrl { get; set; } = null!;

		[Required]
		[StringLength(LeagueNameMaxLength, ErrorMessage = LeagueNameCannotExceed100Characters)]
		public string LeagueName { get; set; } = null!;

		// Navigation properties for matches
		public ICollection<MatchResult> HomeMatches { get; set; } = new List<MatchResult>();
		public ICollection<MatchResult> AwayMatches { get; set; } = new List<MatchResult>();
		public ICollection<TeamAlias> Aliases { get; set; } = new List<TeamAlias>();

	}
}
