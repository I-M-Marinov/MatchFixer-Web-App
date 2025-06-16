using MatchFixer.Core.DTOs.Bets;


namespace MatchFixer.Core.Contracts
{
	public interface IBettingService
	{
		Task<(string Message, bool IsSuccess)> PlaceBetAsync(Guid userId, BetSlipDto betSlipDto);
		Task<IEnumerable<UserBetSlipDTO>> GetBetsByUserAsync(Guid userId);
	}
}
