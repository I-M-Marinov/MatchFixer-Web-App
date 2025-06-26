using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Bets;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using MatchFixer.Infrastructure.Contracts;

namespace MatchFixer.Core.Services;

public class BettingService : IBettingService
{
	private readonly MatchFixerDbContext _dbContext;
	private readonly ISessionService _sessionService;
	private readonly ITimezoneService _timezoneService;
	private readonly IWalletService _walletService;

	public BettingService(MatchFixerDbContext dbContext, 
		ISessionService sessionService, 
		ITimezoneService timezoneService,
		IWalletService walletService)
	{
		_dbContext = dbContext;
		_sessionService = sessionService;
		_timezoneService = timezoneService;
		_walletService = walletService;
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

		// Deduct the amount from the user's wallet if has enough money to place the bet
		var (success, message) = await _walletService.DeductForBetAsync(userId, betSlipDto.Amount);

		if (!success)
		{
			return (message, false);
		}

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
			.ThenInclude(b => b.MatchEvent)
			.ThenInclude(me => me.HomeTeam)
			.Include(bs => bs.Bets)
			.ThenInclude(b => b.MatchEvent)
			.ThenInclude(me => me.AwayTeam)
			.Where(bs => bs.UserId == userId)
			.ToListAsync();

		var timeZoneId = _sessionService.GetUserTimezone();

		return betSlips.Select(bs => new UserBetSlipDTO
		{
			Id = bs.Id,
			BetTime = bs.BetTime,
			DisplayTime = _timezoneService.FormatForUserBets(bs.BetTime, timeZoneId),
			Amount = bs.Amount,
			UserId = bs.UserId,
			WinAmount = bs.WinAmount,
			TotalOdds = bs.Bets.Any()
				? bs.Bets.Select(b => b.Odds).Aggregate((acc, odd) => acc * odd)
				: 1,
			Status = bs.IsSettled
				? (bs.WinAmount > 0 ? "Won" : "Lost")
				: "Pending",

			Bets = bs.Bets.Select(b => new SingleBetDto
			{
				MatchId = b.MatchEventId,
				HomeTeam = b.MatchEvent.HomeTeam.Name,
				AwayTeam = b.MatchEvent.AwayTeam.Name,
				SelectedOption = b.Pick.ToString(),
				Odds = b.Odds
			}).ToList()
		});
	}

}