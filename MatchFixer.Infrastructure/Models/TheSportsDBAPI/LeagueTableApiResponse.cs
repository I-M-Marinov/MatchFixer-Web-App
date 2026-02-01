using System.Text.Json.Serialization;

namespace MatchFixer.Infrastructure.Models.TheSportsDBAPI
{
	public class LeagueTableApiResponse
	{
		[JsonPropertyName("table")]
		public List<LeagueTableRowApiDto> Table { get; set; }
	}
}
