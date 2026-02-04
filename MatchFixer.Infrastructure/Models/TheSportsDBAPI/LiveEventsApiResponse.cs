
using System.Text.Json.Serialization;


namespace MatchFixer.Infrastructure.Models.TheSportsDBAPI
{
	public class LiveEventsApiResponse
	{
		[JsonPropertyName("events")]
		public List<LiveEventApiDto>? Events { get; set; }
	}
}
