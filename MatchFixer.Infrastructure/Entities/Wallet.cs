using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatchFixer.Infrastructure.Entities
{
	public class Wallet
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public Guid UserId { get; set; }

		[ForeignKey("UserId")]
		public ApplicationUser User { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal Balance { get; set; } = 0m;
		public bool IsLocked { get; set; } = false;
		public DateTime? LockedAtUtc { get; set; } // locked time in UTC
		public string? ReasonForLock { get; set; } // reason for the lock 

		[MaxLength(3)]
		public string Currency { get; set; } = "EUR"; // default currently Euro
		public DateTime? HistoryClearedAt { get; set; } // history clear time in UTC 
		public DateTime? HistoryClearedFromAdminAt { get; set; } // history clear time in UTC if the admin did it 

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // create time in UTC 

		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // updated time in UTC

		public ICollection<WalletTransaction> Transactions { get; set; }
	}
}
