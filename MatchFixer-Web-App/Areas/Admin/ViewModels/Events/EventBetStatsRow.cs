namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Events
{
	public class EventBetStatsRow
	{
		public Guid EventId { get; set; }
		public string HomeTeam { get; set; } = "";
		public string AwayTeam { get; set; } = "";
		public bool IsLive { get; set; }
		public string? HomeTeamLogoUrl { get; set; } = ""; // API
		public string? AwayTeamLogoUrl { get; set; } = ""; // API
		public string? HomeTeamLocalLogoUrl { get; set; } // LOCAL
		public string? AwayTeamLocalLogoUrl { get; set; } // LOCAL
		public string? EffectiveHomeTeamLogo =>
			!string.IsNullOrWhiteSpace(HomeTeamLocalLogoUrl)
				? HomeTeamLocalLogoUrl
				: HomeTeamLogoUrl;

		public string? EffectiveAwayTeamLogo =>
			!string.IsNullOrWhiteSpace(AwayTeamLocalLogoUrl)
				? AwayTeamLocalLogoUrl
				: AwayTeamLogoUrl;
		public string LeagueName { get; set; } = "";
		public DateTime? KickoffUtc { get; set; }
		public string? CompetitionName { get; set; }

		// amounts
		public decimal TotalStake { get; set; }
		public decimal HomeStake { get; set; }
		public decimal DrawStake { get; set; }
		public decimal AwayStake { get; set; }

		// counts 
		public int TotalBets { get; set; }
		public int HomeBets { get; set; }
		public int DrawBets { get; set; }
		public int AwayBets { get; set; }

		// % (0..100)
		public decimal HomePct { get; set; }
		public decimal DrawPct { get; set; }
		public decimal AwayPct { get; set; }

		// Breakdown of the extra betting markets (Over/Under, BTTS, Double Chance)
		// shown in the expandable accordion. Empty when the event has no market bets.
		public List<EventMarketBreakdown> Markets { get; set; } = new();
	}

	public sealed class EventMarketBreakdown
	{
		public MatchFixer.Common.Enums.BettingMarket Market { get; set; }
		public string MarketName { get; set; } = "";
		public int TotalBets { get; set; }
		public decimal TotalStake { get; set; }
		public List<EventMarketSelectionStat> Selections { get; set; } = new();
	}

	public sealed class EventMarketSelectionStat
	{
		public string Selection { get; set; } = "";
		public string Label { get; set; } = "";
		public int Bets { get; set; }
		public decimal Stake { get; set; }
		public decimal Pct { get; set; }
	}
}
