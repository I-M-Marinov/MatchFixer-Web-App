namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Events
{
	public class AdminEventHistoryFilters
	{
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public string? League { get; set; }
		public Guid? TeamId { get; set; }
	}
}
