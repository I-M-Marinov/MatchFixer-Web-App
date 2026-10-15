using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using MatchFixer.Common.Enums;

namespace MatchFixer.Infrastructure.Entities
{
	public class Tournament
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		[MaxLength(100)]
		public string Name { get; set; }

		[MaxLength(1000)]
		public string? Description { get; set; }

		// Real-money cost to enter. All entry fees form the prize pool.
		[Column(TypeName = "decimal(18,2)")]
		public decimal EntryFee { get; set; }

		// Play-money chips each entrant starts with (ranking only).
		[Column(TypeName = "decimal(18,2)")]
		public decimal StartingBankroll { get; set; } = 1000m;

		// House cut taken off the prize pool before payouts (e.g. 10.00 = 10%).
		[Column(TypeName = "decimal(5,2)")]
		public decimal RakePercent { get; set; } = 10m;

		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public TournamentStatus Status { get; set; } = TournamentStatus.Upcoming;

		// Below this count the tournament auto-cancels at start/close and refunds entry fees.
		public int MinEntrantsToRun { get; set; } = 2;

		// Optional cap on how many people can join (null = unlimited).
		public int? MaxEntrants { get; set; }

		// Minimum number of matches (legs) every tournament bet must include. 1 = singles allowed.
		public int MinBetLegs { get; set; } = 1;

		[Required]
		public Guid CreatedByUserId { get; set; }

		[ForeignKey(nameof(CreatedByUserId))]
		public ApplicationUser CreatedBy { get; set; }

		public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

		// Idempotency guard: set once the prize pool has been paid out / refunded.
		public bool PrizePoolSettled { get; set; }
		public DateTime? SettledAtUtc { get; set; }

		public ICollection<TournamentEntry> Entries { get; set; } = new List<TournamentEntry>();
		public ICollection<TournamentMessage> Messages { get; set; } = new List<TournamentMessage>();
	}
}
