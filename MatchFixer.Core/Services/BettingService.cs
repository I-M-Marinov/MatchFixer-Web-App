using MatchFixer.Core.DTOs;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Bets;
using MatchFixer.Infrastructure;
using Microsoft.EntityFrameworkCore;

public class BettingService : IBettingService
{
	private readonly MatchFixerDbContext _dbContext;

	public BettingService(MatchFixerDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<(string Message, bool IsSuccess)> PlaceBetAsync(Guid userId, BetSlipDto betSlipDto)
	{
		if (betSlipDto == null || betSlipDto.Bets == null || !betSlipDto.Bets.Any())
			return ("No bets provided.", false);

		if (betSlipDto.Amount <= 0)
			return ("Bet amount must be greater than zero.", false);

		foreach (var betDto in betSlipDto.Bets)
		{
			// Validate match event exists
			var matchEvent = await _dbContext.MatchEvents.FindAsync(betDto.MatchId);
			if (matchEvent == null)
				return ($"Match with ID {betDto.MatchId} not found.", false);

			// Validate pick is a known enum value
			if (!Enum.TryParse<MatchPick>(betDto.SelectedOption, true, out var parsedPick))
				return ($"Invalid pick option '{betDto.SelectedOption}'.", false);

			// Create the Bet entity
			var bet = new Bet
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				MatchEventId = betDto.MatchId,
				Pick = parsedPick,
				Amount = betSlipDto.Amount,
				WinAmount = betSlipDto.Amount * betDto.Odds,
				IsSettled = false,
				BetTime = DateTime.UtcNow
			};

			await _dbContext.Bets.AddAsync(bet);
		}

		await _dbContext.SaveChangesAsync();
		return ("Bet(s) successfully placed.", true);
	}
}