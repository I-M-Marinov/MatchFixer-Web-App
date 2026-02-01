using System.Text.Json.Serialization;

public class LeagueTableRowApiDto
{
	[JsonPropertyName("intRank")]
	public string Rank { get; set; } = "0";

	[JsonPropertyName("strTeam")]
	public string Team { get; set; } = string.Empty;

	[JsonPropertyName("strBadge")]
	public string Badge { get; set; } = string.Empty;

	[JsonPropertyName("intPlayed")]
	public string Played { get; set; } = "0";

	[JsonPropertyName("intWin")]
	public string Wins { get; set; } = "0";

	[JsonPropertyName("intDraw")]
	public string Draws { get; set; } = "0";

	[JsonPropertyName("intLoss")]
	public string Losses { get; set; } = "0";

	[JsonPropertyName("intGoalDifference")]
	public string GoalDifference { get; set; } = "0";

	[JsonPropertyName("intPoints")]
	public string Points { get; set; } = "0";
}