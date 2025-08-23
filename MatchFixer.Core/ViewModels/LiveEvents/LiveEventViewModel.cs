using MatchFixer.Infrastructure.Entities;

namespace MatchFixer.Core.ViewModels.LiveEvents
{
	public class LiveEventViewModel
	{

		public Guid Id { get; set; }
		public string HomeTeam { get; set; }
		public string AwayTeam { get; set; }
		public DateTime KickoffTime { get; set; }

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

		public string? HomeTeamLogoUrl { get; set; }
		public string? AwayTeamLogoUrl { get; set; }
		public bool IsDerby { get; set; }
		public string UserTimeZone { get; set; }
		public bool IsCancelled { get; set; }
		public string MatchStatus { get; set; } = "Scheduled";

	}
}