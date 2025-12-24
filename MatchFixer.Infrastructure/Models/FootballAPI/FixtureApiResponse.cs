namespace MatchFixer.Infrastructure.Models.FootballAPI
{
	public class FixtureApiResponse
	{
		public List<FixtureData> Response { get; set; }
		public ApiPaging Paging { get; set; } = new();

	}
}
