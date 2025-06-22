using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MatchFixer.Common.Enums;

namespace MatchFixer.Infrastructure.Entities
{
	public class WalletTransaction
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public Guid WalletId { get; set; }

		[ForeignKey("WalletId")]
		public Wallet Wallet { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal Amount { get; set; }

		[Required]
		public WalletTransactionType TransactionType { get; set; }

		[MaxLength(255)]
		public string Reference { get; set; }

		[MaxLength(255)]
		public string Description { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // creation time in UTC 
	}
}
