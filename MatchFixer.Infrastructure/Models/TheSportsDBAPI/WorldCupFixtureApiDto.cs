using System.Text.Json.Serialization;

namespace MatchFixer.Infrastructure.Models.TheSportsDBAPI
{
	public class WorldCupFixtureApiDto
	{
		[JsonPropertyName("idEvent")]
		public string EventId { get; set; } = string.Empty;

		[JsonPropertyName("strHomeTeam")]
		public string HomeTeam { get; set; } = string.Empty;

		[JsonPropertyName("strAwayTeam")]
		public string AwayTeam { get; set; } = string.Empty;

		[JsonPropertyName("strHomeTeamBadge")]
		public string? HomeBadge { get; set; }

		[JsonPropertyName("strAwayTeamBadge")]
		public string? AwayBadge { get; set; }

		[JsonPropertyName("dateEvent")]
		public string Date { get; set; } = string.Empty;

		[JsonPropertyName("strTime")]
		public string Time { get; set; } = string.Empty;

		// strRound does not exist in the TheSportsDB response — the API
		// uses intRound (a numeric matchday / knockout-round index).
		[JsonPropertyName("intRound")]
		public string Round { get; set; } = string.Empty;

		[JsonPropertyName("intHomeScore")]
		public string? HomeScore { get; set; }

		[JsonPropertyName("intAwayScore")]
		public string? AwayScore { get; set; }

		[JsonPropertyName("strStatus")]
		public string? Status { get; set; }
	}
}