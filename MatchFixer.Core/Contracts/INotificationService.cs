namespace MatchFixer.Core.Contracts
{
	public interface INotificationService
	{
		Task NotifyUsersForMatchAsync(Guid matchId);
	}
}
