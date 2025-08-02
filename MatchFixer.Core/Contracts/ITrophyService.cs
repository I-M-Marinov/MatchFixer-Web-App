namespace MatchFixer.Core.Contracts
{
	public interface ITrophyService
	{
		Task AwardTrophyIfNotAlreadyAsync(Guid userId, int trophyId, string? notes = null);
		Task EvaluateTrophiesAsync(Guid userId);
	}
}
