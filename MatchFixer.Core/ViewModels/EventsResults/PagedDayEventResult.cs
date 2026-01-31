namespace MatchFixer.Core.ViewModels.EventsResults
{
	public class PagedDayEventResult<T>
	{
		public DateOnly Day { get; init; }
		public IReadOnlyList<T> Items { get; init; } = [];

		public int DayIndex { get; init; }          
		public int TotalDays { get; init; }
		public DateOnly TodayLocal { get; set; }
		public bool HasPreviousDay => DayIndex < TotalDays - 1;
		public bool HasNextDay => DayIndex > 0;
	}

}
