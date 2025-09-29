using MatchFixer_Web_App.Areas.Admin.ViewModels.Bets;

namespace MatchFixer_Web_App.Areas.Admin.Interfaces
{
	public interface IAdminUserBetsService
	{
		Task<AdminUserBetsColumnsViewModel?> GetColumnsAsync(
			Guid userId,
			string timeZoneId,
			int maxPerColumn = 200);
	}
}
