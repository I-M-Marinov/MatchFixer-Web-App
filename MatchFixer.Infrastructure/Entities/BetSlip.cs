using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using static MatchFixer.Common.ValidationConstants.BetValidations;


namespace MatchFixer.Infrastructure.Entities
{
	public class BetSlip
	{
		[Key]
		[Comment("Unique identifier for a BetSlip")]
		public Guid Id { get; set; }

		[Required(ErrorMessage = AmountIsRequired)]
		[Range(1.00, 50000, ErrorMessage = AmountMustBeBetweenOneAndFiftyThousand)]
		[Comment("Amount of the Bet")]
		public decimal Amount { get; set; }

		[Required]
		[Comment("Id of the user who placed the bet slip")]
		public Guid UserId { get; set; }

		[ForeignKey(nameof(UserId))]
		public ApplicationUser User { get; set; }

		[Required]
		[Comment("Time the bet slip was placed")]
		public DateTime BetTime { get; set; } = DateTime.UtcNow;

		[Comment("Indicates whether the bet slip has been settled")]
		public bool IsSettled { get; set; }

		[Comment("The total win amount for the bet slip if successful")]
		public decimal? WinAmount { get; set; }

		// Navigation property for all bets
		public ICollection<Bet> Bets { get; set; } = new List<Bet>();
	}

}
