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

		[MaxLength(3)]
		public string Currency { get; set; } = "EUR"; // default currently Euro

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // create time in UTC 

		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // updated time in UTC

		public ICollection<WalletTransaction> Transactions { get; set; }
	}
}
