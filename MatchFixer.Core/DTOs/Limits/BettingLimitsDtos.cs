using MatchFixer.Common.Enums;

namespace MatchFixer.Core.DTOs.Limits
{
	/// <summary>One leg of a slip, as needed to work out its limits.</summary>
	public record SlipLeg(Guid MatchId, BettingMarket Market);

	/// <summary>Limits that apply to a slip (null = no limit).</summary>
	public class SlipLimitsDto
	{
		/// <summary>Number of different matches on the slip.</summary>
		public int Legs { get; set; }

		/// <summary>True when the slip contains an Over/Under, BTTS or Double Chance pick.</summary>
		public bool HasExtraMarkets { get; set; }

		/// <summary>Stake limit from the league x legs table (averaged across the slip's leagues).</summary>
		public decimal? MaxStake { get; set; }

		/// <summary>Maximum potential return (averaged across the slip's leagues).</summary>
		public decimal? MaxWin { get; set; }

		/// <summary>The stake that reaches <see cref="MaxWin"/> at the slip's total odds.</summary>
		public decimal? MaxStakeForWin { get; set; }

		/// <summary>
		/// Max stake of the odds-boost promo on the slip (lowest when several). When set it overrides
		/// the league limits and max win: the slip can only take what the promo allows.
		/// </summary>
		public decimal? PromoMaxStake { get; set; }

		/// <summary>
		/// The stake limit that applies: the promo's max stake when the slip has a capped promo,
		/// otherwise the lower of <see cref="MaxStake"/> and <see cref="MaxStakeForWin"/>.
		/// </summary>
		public decimal? EffectiveMaxStake { get; set; }
	}

	/// <summary>Site-wide market switches.</summary>
	public class MarketSwitchesDto
	{
		public bool OverUnderEnabled { get; set; } = true;
		public bool BothTeamsToScoreEnabled { get; set; } = true;
		public bool DoubleChanceEnabled { get; set; } = true;

		public bool IsEnabled(BettingMarket market) => market switch
		{
			BettingMarket.OverUnderGoals => OverUnderEnabled,
			BettingMarket.BothTeamsToScore => BothTeamsToScoreEnabled,
			BettingMarket.DoubleChance => DoubleChanceEnabled,
			_ => true // Match Result is always offered
		};
	}

	/// <summary>One row of the admin limits table (blank values = use Default / no limit).</summary>
	public class LeagueLimitRowDto
	{
		public string LeagueName { get; set; } = string.Empty;

		public decimal? MaxStake1 { get; set; }
		public decimal? MaxStake2 { get; set; }
		public decimal? MaxStake3 { get; set; }
		public decimal? MaxStake4 { get; set; }
		public decimal? MaxStake5Plus { get; set; }

		public decimal? ExtraMaxStake1 { get; set; }
		public decimal? ExtraMaxStake2 { get; set; }
		public decimal? ExtraMaxStake3 { get; set; }
		public decimal? ExtraMaxStake4 { get; set; }
		public decimal? ExtraMaxStake5Plus { get; set; }

		public decimal? MaxWin { get; set; }

		public DateTime? UpdatedAtUtc { get; set; }
		public Guid? UpdatedByUserId { get; set; }
	}

	/// <summary>Everything the admin Limits page shows and saves.</summary>
	public class BettingLimitsConfigDto
	{
		public MarketSwitchesDto Markets { get; set; } = new();

		/// <summary>The Default row first, then leagues alphabetically.</summary>
		public List<LeagueLimitRowDto> Leagues { get; set; } = new();

		public DateTime? MarketsUpdatedAtUtc { get; set; }
		public Guid? MarketsUpdatedByUserId { get; set; }
	}
}
