using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatchFixer.Infrastructure.Entities
{
	public class TournamentMessage
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public Guid TournamentId { get; set; }

		[ForeignKey(nameof(TournamentId))]
		public Tournament Tournament { get; set; }

		[Required]
		public Guid UserId { get; set; }

		[ForeignKey(nameof(UserId))]
		public ApplicationUser User { get; set; }

		[Required]
		[MaxLength(1000)]
		public string Text { get; set; }

		public DateTime PostedAtUtc { get; set; } = DateTime.UtcNow;

		// Admin soft-delete for moderation.
		public bool IsDeleted { get; set; }
	}
}
