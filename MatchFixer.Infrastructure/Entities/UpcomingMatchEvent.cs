using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatchFixer.Infrastructure.Entities
{
	public class UpcomingMatchEvent
	{
		[Key]
		public Guid Id { get; set; }

		// API identity
		[Required]
		public int ApiFixtureId { get; set; }

		[Required]
		public int ApiLeagueId { get; set; }

		[Required]
		public DateTime MatchDateUtc { get; set; }

		// Teams
		[Required]
		public Guid HomeTeamId { get; set; }

		[Required]
		public Guid AwayTeamId { get; set; }

		[ForeignKey(nameof(HomeTeamId))]
		public Team HomeTeam { get; set; } = null!;

		[ForeignKey(nameof(AwayTeamId))]
		public Team AwayTeam { get; set; } = null!;

		// State
		public bool IsCancelled { get; set; }
		public bool IsImported { get; set; } 

		// Audit
		public DateTime ImportedAtUtc { get; set; }
	}

}
