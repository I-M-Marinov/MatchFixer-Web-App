using MatchFixer.Infrastructure.Entities;

namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Email
{
	public class EmailQueueViewModel
	{
		public IReadOnlyList<QueuedEmail> Winning { get; set; } = new List<QueuedEmail>();

		public IReadOnlyList<QueuedEmail> Other { get; set; } = new List<QueuedEmail>();

		public int PendingCount { get; set; }
	}
}
