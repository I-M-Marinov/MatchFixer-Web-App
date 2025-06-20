namespace MatchFixer.Core.Contracts
{
	public interface IUserContextService
	{
		Guid GetUserId();
		string GetUserTimeZone();
	}
}
