using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.ChangeRequests;
using Microsoft.AspNetCore.SignalR;

namespace MatchFixer_Web_App.Areas.Admin.Hubs
{
	public sealed class ChangeRequestNotifier : IChangeRequestNotifier
	{
		private readonly IHubContext<AdminReviewHub> _hub;
		private readonly ILogger<ChangeRequestNotifier> _logger;

		public ChangeRequestNotifier(IHubContext<AdminReviewHub> hub, ILogger<ChangeRequestNotifier> logger)
		{
			_hub = hub;
			_logger = logger;
		}

		public Task RequestCreatedAsync(ChangeRequestInfo r, int pendingCount) =>
			SafeSend("ChangeRequestCreated", new
			{
				id = r.Id,
				userName = r.UserName,
				userEmail = r.UserEmail,
				typeLabel = r.TypeLabel,
				summary = r.Summary,
				requestedAt = r.RequestedAtUtc.ToString("dd MMM yyyy, HH:mm") + " UTC",
				pendingCount
			});

		public Task RequestResolvedAsync(Guid requestId, string outcome, int pendingCount) =>
			SafeSend("ChangeRequestResolved", new { id = requestId, outcome, pendingCount });

		private async Task SafeSend(string method, object payload)
		{
			try
			{
				await _hub.Clients.All.SendAsync(method, payload);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Could not push {Method} to admins", method);
			}
		}
	}
}
