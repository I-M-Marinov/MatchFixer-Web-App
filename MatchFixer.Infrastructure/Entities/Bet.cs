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

		[Required(ErrorMessage = PickIsRequired)]
		[Comment("The chosen outcome the user will choose for the bet")]
		public MatchPick Pick { get; set; }

		[Required]
		[Comment("The exact time the bet was placed")]
		public DateTime BetTime { get; set; } = DateTime.UtcNow;

		[Required]
		[Comment("Id of match event user is placing a bet for")]

		public Guid MatchEventId { get; set; }

		[ForeignKey(nameof(MatchEventId))]
		public MatchEvent MatchEvent { get; set; }

		[Required]
		public Guid BetSlipId { get; set; }

		[ForeignKey(nameof(BetSlipId))]
		public BetSlip BetSlip { get; set; }

		[Comment("Betting market of this bet (0 = Match Result)")]
		public BettingMarket Market { get; set; } = BettingMarket.MatchResult;

		[StringLength(10)]
		[Comment("Selection code for non 1X2 markets (null for Match Result, which uses Pick)")]
		public string? Selection { get; set; }

		[Comment("Goals line for Over/Under bets")]
		public decimal? Line { get; set; }

		[Comment("Odds for this particular pick")]
		public decimal Odds { get; set; }

		[Required]
		[Comment("Status of the bet")]
		public BetStatus Status { get; set; } = BetStatus.Pending; // default to pending status 

		public Guid? OddsBoostId { get; set; }
		[ForeignKey(nameof(OddsBoostId))]
		public OddsBoost? OddsBoost { get; set; }
	}
}
