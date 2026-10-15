using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatchFixer.Infrastructure.Entities
{
	public class TournamentEntry
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

		// Current play-money chips. Starts at Tournament.StartingBankroll,
		// moves only through tournament bet placement/settlement.
		[Column(TypeName = "decimal(18,2)")]
		public decimal Bankroll { get; set; }

		// Real-money entry fee actually charged when joining (snapshot of the fee).
		[Column(TypeName = "decimal(18,2)")]
		public decimal EntryFeePaid { get; set; }

		public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;

		// Set at settlement.
		public int? FinalRank { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal? PrizeAwarded { get; set; }

		// Tournament bet slips funded by this entry's bankroll.
		public ICollection<BetSlip> BetSlips { get; set; } = new List<BetSlip>();
	}
}
