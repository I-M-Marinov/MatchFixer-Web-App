using MatchFixer.Core.ViewModels.LiveEvents;

namespace MatchFixer.Core.Contracts
{
	public interface IOddsGeneratorService
	{
		Task<GeneratedOddsDto> GenerateAsync(Guid homeTeamId, Guid awayTeamId, CancellationToken ct = default);
	}
}
