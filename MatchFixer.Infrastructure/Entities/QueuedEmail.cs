using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MatchFixer.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer.Infrastructure.Entities
{
	public class QueuedEmail
	{
		[Key]
		[Comment("Unique identifier for a queued email")]
		public Guid Id { get; set; }

		[Required]
		[MaxLength(256)]
		[Comment("Recipient email address")]
		public string Recipient { get; set; } = null!;

		[Comment("Optional id of the recipient user")]
		public Guid? RecipientUserId { get; set; }

		[ForeignKey(nameof(RecipientUserId))]
		public ApplicationUser? RecipientUser { get; set; }

		[Required]
		[MaxLength(300)]
		[Comment("Email subject")]
		public string Subject { get; set; } = null!;

		[Required]
		[Comment("Email body (HTML) - stored as nvarchar(max)")]
		public string BodyHtml { get; set; } = null!;

		[Comment("Which kind of email this is (Winning, Event, Voided, Birthday, Other)")]
		public EmailQueueCategory Category { get; set; }

		[Comment("Review / send status")]
		public EmailQueueStatus Status { get; set; } = EmailQueueStatus.Pending;

		[Comment("When the email was enqueued (UTC)")]
		public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

		[Comment("Earliest time the dispatcher may send this (UTC) - used for interval sending")]
		public DateTime? SendAfterUtc { get; set; }

		[Comment("When the email was actually sent (UTC)")]
		public DateTime? SentAtUtc { get; set; }

		[Comment("Admin who approved / dropped the email")]
		public Guid? ReviewedByUserId { get; set; }

		[Comment("When an admin approved / dropped the email (UTC)")]
		public DateTime? ReviewedAtUtc { get; set; }

		[Comment("Number of send attempts")]
		public int AttemptCount { get; set; }

		[MaxLength(1000)]
		[Comment("Last send error, if any")]
		public string? LastError { get; set; }

		[MaxLength(250)]
		[Comment("Human-friendly context, e.g. teams or slip id")]
		public string? RelatedInfo { get; set; }
	}
}
