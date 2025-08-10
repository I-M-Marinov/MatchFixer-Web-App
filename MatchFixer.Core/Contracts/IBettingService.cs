using MatchFixer.Core.DTOs.Bets;


namespace MatchFixer.Core.Contracts
{
	public interface IBettingService
	{
		Task<(string Message, bool IsSuccess)> PlaceBetAsync(Guid userId, BetSlipDto betSlipDto, string profileUrl);
		Task<IEnumerable<UserBetSlipDTO>> GetBetsByUserAsync(Guid userId);
		Task<bool> CancelBetsForMatchAsync(Guid matchEventId);
		Task<bool> EvaluateBetSlipAsync(Guid betSlipId);
	}
}
