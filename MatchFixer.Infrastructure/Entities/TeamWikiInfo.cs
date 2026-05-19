namespace MatchFixer.Infrastructure.Entities
{
	public class TeamWikiInfo
	{
		public Guid Id { get; set; }

		public Guid TeamId { get; set; }

		public Team Team { get; set; } = null!;

		public string Summary { get; set; } = string.Empty;

		public string? ImageUrl { get; set; }

		public string WikipediaUrl { get; set; } = string.Empty;

		public DateTime LastUpdatedUtc { get; set; }
	}
}
