using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using MatchFixer.Common.Enums;

namespace MatchFixer.Infrastructure.Entities
{
	// A generic, admin-approved change request. Today it gates profile name changes;
	// new approval types just add a ChangeRequestType value + an "apply" handler.
	public class ChangeRequest
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public Guid UserId { get; set; }

		[ForeignKey(nameof(UserId))]
		public ApplicationUser User { get; set; }

		public ChangeRequestType Type { get; set; }

		// JSON payload with the proposed values (shape depends on Type).
		[Required]
		[MaxLength(4000)]
		public string Payload { get; set; }

		// Human-readable summary shown to the admin (e.g. "John Doe -> Johnny Doe").
		[MaxLength(500)]
		public string? Summary { get; set; }

		public ChangeRequestStatus Status { get; set; } = ChangeRequestStatus.Pending;

		public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;

		public Guid? ReviewedByUserId { get; set; }

		[ForeignKey(nameof(ReviewedByUserId))]
		public ApplicationUser? ReviewedBy { get; set; }

		public DateTime? ReviewedAtUtc { get; set; }

		[MaxLength(1000)]
		public string? ReviewNote { get; set; }
	}
}
