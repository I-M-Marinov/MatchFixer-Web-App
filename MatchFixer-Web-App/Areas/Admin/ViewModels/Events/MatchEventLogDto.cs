namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Events
{
	public class MatchEventLogDto
	{
		public DateTime ChangedAt { get; set; }
		public string PropertyName { get; set; }
		public string? OldValue { get; set; }
		public string? NewValue { get; set; }

		public string ChangedByFullName { get; set; }
		public string ChangedByUserName { get; set; }

		public string ChangedBy => $"{ChangedByFullName} ({ChangedByUserName})";
	}
}
