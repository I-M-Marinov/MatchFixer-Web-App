namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Events
{
	public sealed class EventLeagueStat
	{
		public string  League      { get; init; } = string.Empty;
		public decimal TotalProfit { get; init; }
		public int     EventCount  { get; init; }
	}

	public sealed class PaginatedEventList<T>
	{
		public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
		public int Page { get; init; }
		public int PageSize { get; init; }
		public int TotalCount { get; init; }
		public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(PageSize, 1));
		public Dictionary<string, int> LeagueEventCounts { get; set; } = new();
		public Dictionary<string, int> CompetitionCounts { get; set; } = new();

		// Aggregate totals across ALL filtered events
		public decimal AllEventsStake    { get; init; }
		public decimal AllEventsPayout   { get; init; }
		public decimal AllEventsProfit   => AllEventsStake - AllEventsPayout;
		// Sum of negative per-event profits 
		public decimal AllEventsLoss     { get; init; }
		// Sum of positive per-event profits 
		public decimal AllEventsPositive { get; init; }
		// Per-league breakdown across ALL filtered events
		public List<EventLeagueStat> AllEventsLeagueStats { get; init; } = new();
	}
}
