using MatchFixer.Core.DTOs.Betting;

namespace MatchFixer.Core.Contracts
{
	public interface IBettingEligibilityService
	{
		Task<BettingEligibility> GetEligibilityAsync(Guid userId, CancellationToken ct = default);

		Task<(bool Success, string Message)> SelfVerifyAgeAsync(Guid userId, DateTime dateOfBirth, CancellationToken ct = default);

		Task<(bool Success, string Message)> AdminVerifyAgeAsync(Guid userId, DateTime? dateOfBirth, CancellationToken ct = default);

		Task<(bool Success, string Message)> AdminResetAgeVerificationAsync(Guid userId, CancellationToken ct = default);

		Task<(bool Success, string Message)> AdminSetBettingBlockedAsync(Guid userId, bool blocked, CancellationToken ct = default);
	}
}
