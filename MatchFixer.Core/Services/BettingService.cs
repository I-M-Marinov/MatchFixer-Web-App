using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Bets;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
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

		// Create the BetSlip first
		var betSlip = new BetSlip
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			Amount = betSlipDto.Amount,
			BetTime = DateTime.UtcNow,
			IsSettled = false
		};

		foreach (var betDto in betSlipDto.Bets)
		{
			// Validate match event exists
			var matchEvent = await _dbContext.MatchEvents.FindAsync(betDto.MatchId);
			if (matchEvent == null)
				return ($"Match with ID {betDto.MatchId} not found.", false);

			// Validate pick is a known enum value
			if (!Enum.TryParse<MatchPick>(betDto.SelectedOption, true, out var parsedPick))
				return ($"Invalid pick option '{betDto.SelectedOption}'.", false);

			// Create the Bet entity (no amount per individual bet anymore)
			var bet = new Bet
			{
				Id = Guid.NewGuid(),
				BetSlipId = betSlip.Id,
				MatchEventId = matchEvent.Id,
				Pick = parsedPick,
				Odds = betDto.Odds,
				BetTime = DateTime.UtcNow
			};

			betSlip.Bets.Add(bet);
		}

		await _dbContext.BetSlips.AddAsync(betSlip);
		await _dbContext.SaveChangesAsync();

		return ("Your betting slip was successfully submitted! Good Luck!", true);
	}

	public async Task<IEnumerable<UserBetSlipDTO>> GetBetsByUserAsync(Guid userId)
	{
		var betSlips = await _dbContext.BetSlips
			.Include(bs => bs.Bets) 
			.Where(bs => bs.UserId == userId)
			.ToListAsync();

		return betSlips.Select(bs => new UserBetSlipDTO
		{
			Id = bs.Id,
			BetTime = bs.BetTime,
			Amount = bs.Amount,
			UserId = bs.UserId,
			WinAmount = bs.WinAmount,
			TotalOdds = bs.Bets.Any() ? bs.Bets.Select(b => b.Odds).Aggregate((acc, odd) => acc * odd) : 1,
			Status = bs.IsSettled ? (bs.WinAmount > 0 ? "Won" : "Lost") : "Pending"
		});
	}

}