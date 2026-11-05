using MatchFixer.Core.ViewModels.ChangeRequests;

namespace MatchFixer.Core.Contracts
{
	public interface IChangeRequestNotifier
	{
		Task RequestCreatedAsync(ChangeRequestInfo request, int pendingCount);

		Task RequestResolvedAsync(Guid requestId, string outcome, int pendingCount);
	}
}
