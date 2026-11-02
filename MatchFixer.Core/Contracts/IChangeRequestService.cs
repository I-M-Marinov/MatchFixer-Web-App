using MatchFixer.Core.ViewModels.ChangeRequests;

namespace MatchFixer.Core.Contracts
{
	public interface IChangeRequestService
	{
		// User submits a name change for approval. Fails if invalid or one is already pending.
		Task<(bool Success, string Message)> SubmitNameChangeAsync(Guid userId, string firstName, string lastName);

		// The user's pending name-change request (for the profile status), or null.
		Task<ChangeRequestInfo?> GetPendingNameChangeAsync(Guid userId);

		// Admin: all pending requests (oldest first).
		Task<IReadOnlyList<ChangeRequestInfo>> GetPendingAsync();

		// Admin: number of pending requests (for the badge).
		Task<int> GetPendingCountAsync();

		// Admin: approve (applies the change) or reject.
		Task<(bool Success, string Message)> ApproveAsync(Guid requestId, Guid adminId, string? note);
		Task<(bool Success, string Message)> RejectAsync(Guid requestId, Guid adminId, string? note);
	}
}
