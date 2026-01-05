namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Events
{
	public sealed class PaginatedEventList<T>
	{
		public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
		public int Page { get; init; }
		public int PageSize { get; init; }
		public int TotalCount { get; init; }
		public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(PageSize, 1));
		public Dictionary<string, int> LeagueEventCounts { get; set; } = new();

	}
}
