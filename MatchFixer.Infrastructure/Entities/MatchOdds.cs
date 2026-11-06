using MatchFixer.Common.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatchFixer.Infrastructure.Entities
{
	/// <summary>
	/// Odds for the extra markets (Over/Under, BTTS, Double Chance).
	/// Match Result (1X2) odds stay on MatchEvent.HomeOdds / DrawOdds / AwayOdds.
	/// </summary>
	public class MatchOdds
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public Guid MatchEventId { get; set; }

		[ForeignKey(nameof(MatchEventId))]
		public MatchEvent MatchEvent { get; set; } = null!;

		[Comment("Betting market this price belongs to")]
		public BettingMarket Market { get; set; }

		[Required]
		[StringLength(10)]
		[Comment("Selection code within the market, e.g. Over, Yes, 1X")]
		public string Selection { get; set; } = null!;

		[Comment("Goals line for Over/Under markets, null otherwise")]
		public decimal? Line { get; set; }

		[Comment("Decimal odds for this selection")]
		public decimal Odds { get; set; }
	}
}
