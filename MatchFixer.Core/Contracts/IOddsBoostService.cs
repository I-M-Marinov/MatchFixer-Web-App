using MatchFixer.Infrastructure.Entities;

namespace MatchFixer.Core.Contracts
{
	public interface IOddsBoostService
	{
		Task<(decimal? home, decimal? draw, decimal? away, OddsBoost? boost)>
			GetEffectiveOddsAsync(Guid matchEventId, decimal? baseHome, decimal? baseDraw, decimal? baseAway,
				CancellationToken ct = default);

		Task<OddsBoost?> CreateOddsBoostAsync(
			Guid matchEventId,
			decimal boostValue,
			TimeSpan duration,
			Guid createdByUserId,
			DateTime? startUtc = null,
			decimal? maxStakePerBet = null,
			int? maxUsesPerUser = null,
			string? note = null,
			CancellationToken ct = default);

		Task<OddsBoost?> StopOddsBoostAsync(Guid oddsBoostId, Guid stoppedByUserId, CancellationToken ct = default);
	}
}
