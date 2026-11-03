using MatchFixer.Core.ViewModels.ChangeRequests;

namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Email
{
	public class ReviewCenterViewModel : EmailQueueViewModel
	{
		public IReadOnlyList<ChangeRequestInfo> ChangeRequests { get; set; } = new List<ChangeRequestInfo>();

		public int ChangeRequestsPendingCount { get; set; }
	}
}
