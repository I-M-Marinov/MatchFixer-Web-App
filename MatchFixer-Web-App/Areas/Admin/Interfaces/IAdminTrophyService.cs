namespace MatchFixer_Web_App.Areas.Admin.Interfaces
{
	public interface IAdminTrophyService
	{
		Task ReevaluateUserTrophiesAsync(Guid userId);
		Task ReevaluateAllUsersAsync();
	}

}
