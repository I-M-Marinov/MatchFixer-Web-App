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
	}

}
