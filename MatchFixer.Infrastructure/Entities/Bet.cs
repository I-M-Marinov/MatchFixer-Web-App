using MatchFixer.Common.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MatchFixer.Common.ValidationConstants.BetValidations;

namespace MatchFixer.Infrastructure.Entities
{
	public class Bet
	{
		[Key]
		[Comment("Unique identifier for a Bet")]
		public Guid Id { get; set; }

		[Required(ErrorMessage = AmountIsRequired)]
		[Range(1.00, 50000, ErrorMessage = AmountMustBeBetweenOneAndFiftyThousand)]
		[Comment("Amount of the Bet")]
		public decimal Amount { get; set; }

		[Required(ErrorMessage = PickIsRequired)]
		[Comment("The chosen outcome the user will choose for the bet")]
		public MatchPick Pick { get; set; }

		[Required]
		[Comment("The exact time the bet was placed")]
		public DateTime BetTime { get; set; } = DateTime.UtcNow;

		[Required]
		[Comment("Id of the user that placed the bet")]
		public Guid UserId { get; set; }

		[ForeignKey(nameof(UserId))]
		public ApplicationUser User { get; set; }

		[Required]
		[Comment("Id of match event user is placing a bet for")]

		public Guid MatchEventId { get; set; }

		[ForeignKey(nameof(MatchEventId))]
		public MatchEvent MatchEvent { get; set; }

		[Comment("Amount that would be won on the bet")]
		public decimal? WinAmount { get; set; }

		[Comment("Signifies if the bet has been settled or not")]
		public bool IsSettled { get; set; }
	}
}
