using MatchFixer.Common.Enums;
using MatchFixer.Infrastructure.Entities;

namespace MatchFixer.Core.Contracts
{
	public interface IEmailQueueService
	{
		Task EnqueueAsync(string recipient, string subject, string bodyHtml, EmailQueueCategory category,
			Guid? recipientUserId = null, string? relatedInfo = null);

		Task<int> GetPendingCountAsync();

		Task<IReadOnlyList<QueuedEmail>> GetReviewQueueAsync();

		Task<QueuedEmail?> GetAsync(Guid id);

		Task<bool> UpdateContentAsync(Guid id, string subject, string bodyHtml);

		Task<int> DropAsync(IEnumerable<Guid> ids, Guid adminId);

		Task<int> SendNowAsync(IEnumerable<Guid> ids, Guid adminId);

		Task<int> ScheduleAsync(IEnumerable<Guid> ids, int intervalSeconds, Guid adminId);
	}
}
