namespace MatchFixer.Infrastructure.Models.Wikipedia
{
	public class WikiTeamInfo
	{
		public string Name { get; set; } = string.Empty;
		public string Summary { get; set; } = string.Empty;
		public string? ImageUrl { get; set; }
		public string WikipediaUrl { get; set; } = string.Empty;
		public List<string> Players { get; set; } = new List<string>();

	}
}
