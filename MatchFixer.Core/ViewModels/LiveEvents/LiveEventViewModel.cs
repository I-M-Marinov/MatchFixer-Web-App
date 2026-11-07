using MatchFixer.Common.Enums;
using MatchFixer.Infrastructure.Entities;

namespace MatchFixer.Core.ViewModels.LiveEvents
{
	public class LiveEventViewModel
	{

		public Guid Id { get; set; }
		public string HomeTeam { get; set; }
		public string AwayTeam { get; set; }
		public DateTime? KickoffTime { get; set; }

		// Base odds
		public decimal HomeWinOdds { get; set; }
		public decimal DrawOdds { get; set; }
		public decimal AwayWinOdds { get; set; }

		// Effective odds (after any boost)
		public decimal? EffectiveHomeWinOdds { get; set; }
		public decimal? EffectiveDrawOdds { get; set; }
		public decimal? EffectiveAwayWinOdds { get; set; }

		// Active boost info (optional)
		public OddsBoost? ActiveBoost { get; set; }
		public DateTime? BoostEndUtc { get; set; }  // passing the end time of the odds boost for countdown timer

		public string? HomeTeamLogoUrl { get; set; } // API
		public string? AwayTeamLogoUrl { get; set; } // API
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

		public bool IsInternational { get; set; }
		public bool IsEuropeanCompetition { get; set; }
		public bool IsDerby { get; set; }
		public string? CompetitionName { get; set; }
		public bool IsCompetitionMatch =>
			!string.IsNullOrWhiteSpace(CompetitionName);
		public string UserTimeZone { get; set; }
		public bool IsCancelled { get; set; }
		public bool IsPostponed { get; set; }
		public bool NoDraw { get; set; }
		public bool HasResult { get; set; }
		public bool IsFavoriteMatch { get; set; }
		public List<string> FavoriteTeams { get; set; } = new();
		public string? LeagueName { get; set; }
		public bool IsFavoriteLeague { get; set; }
		public MatchStatus MatchStatus { get; set; } = MatchStatus.Scheduled;
		public BoostViewModel? BoostActive { get; set; }
		public int? ApiFixtureId { get; set; }

		/// <summary>Extra markets keyed by selection code ("Over", "Under", "Yes", "No", "1X", "12", "X2").</summary>
		public Dictionary<string, decimal> MarketOdds { get; set; } = new();
		public decimal OverUnderLine { get; set; } = MatchFixer.Common.GeneralConstants.MarketSelections.DefaultGoalsLine;
		public bool HasMarketOdds => MarketOdds.Count > 0;

		/// <summary>True when an admin set the extra-market prices by hand.</summary>
		public bool MarketOddsManual { get; set; }

		/// <summary>Per-match switch for the extra markets.</summary>
		public bool ExtraMarketsEnabled { get; set; } = true;
		public bool OverUnderEnabled { get; set; } = true;
		public bool BothTeamsToScoreEnabled { get; set; } = true;
		public bool DoubleChanceEnabled { get; set; } = true;
		public bool IsFromApi => ApiFixtureId != null;


	}
}