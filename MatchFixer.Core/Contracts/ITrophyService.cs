using MatchFixer.Core.ViewModels.Profile;

namespace MatchFixer.Core.Contracts
{
	public interface ITrophyService
	{
		Task<List<TrophyViewModel>> GetAllTrophiesWithUserStatusAsync(Guid userId);
		Task AwardTrophyIfNotAlreadyAsync(Guid userId, int trophyId, string profileUrl, string? notes = null);
		Task EvaluateTrophiesAsync(Guid userId, string profileUrl);
		Task MarkTrophyAsSeenAsync(Guid userId, int trophyId);
	}
}
