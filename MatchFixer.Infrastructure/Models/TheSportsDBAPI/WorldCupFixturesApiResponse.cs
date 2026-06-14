
using System.Text.Json.Serialization;

namespace MatchFixer.Infrastructure.Models.TheSportsDBAPI
{
	public class WorldCupFixturesApiResponse
	{
		[JsonPropertyName("events")]
		public List<WorldCupFixtureApiDto> Events { get; set; }
			= new();
	}
}
