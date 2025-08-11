using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatchFixer.Infrastructure.Entities
{
	public class MatchEventLog
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public Guid MatchEventId { get; set; }

		[ForeignKey(nameof(MatchEventId))]
		public MatchEvent MatchEvent { get; set; }

		[Required]
		[MaxLength(100)]
		public string PropertyName { get; set; } // e.g. "MatchDate", "HomeOdds"

		[MaxLength(500)]
		public string? OldValue { get; set; }

		[MaxLength(500)]
		public string? NewValue { get; set; }

		public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

		[Required]
		public Guid ChangedByUserId { get; set; }

		[ForeignKey(nameof(ChangedByUserId))]
		public ApplicationUser ChangedBy { get; set; }
	}
}
