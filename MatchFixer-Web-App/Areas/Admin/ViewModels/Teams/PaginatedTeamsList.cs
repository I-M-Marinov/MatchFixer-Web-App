namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Teams
{
	public class PaginatedTeamsList<T>
	{
		public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
		public int Page { get; init; }
		public int PageSize { get; init; }
		public int TotalCount { get; init; }
		public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
	}
}
