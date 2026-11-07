using MatchFixer.Common.Enums;
using System.Text.Json.Serialization;

namespace MatchFixer.Core.DTOs.Bets
{
	public class BetSlipItem
	{
		[JsonPropertyName("MatchId")]
		public Guid MatchId { get; set; }
		public bool IsPostponed { get; set; }
		public string? HomeTeam { get; set; }
		public string? AwayTeam { get; set; }
		public string? HomeLogoUrl { get; set; }
		public string? AwayLogoUrl { get; set; }

		[JsonPropertyName("SelectedOption")]
		public string SelectedOption { get; set; }

		public decimal Odds { get; set; }
		public DateTime? StartTimeUtc { get; set; }

		/// <summary>Market of this pick. Accepts "OverUnderGoals" or 1 from the client; defaults to Match Result.</summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public BettingMarket Market { get; set; } = BettingMarket.MatchResult;

		/// <summary>Selection code for extra markets ("Over", "Yes", "1X"...). Null for Match Result.</summary>
		public string? Selection { get; set; }

		public decimal? Line { get; set; }
	}

}
