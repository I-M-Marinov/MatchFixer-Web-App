using MatchFixer.Core.DTOs.Bets;

namespace MatchFixer.Core.Contracts
{
	public interface IAdminInsightsNotifier
	{
		Task PublishBetMixAsync(BetMixUpdateDto payload, CancellationToken ct = default);
	}
}
