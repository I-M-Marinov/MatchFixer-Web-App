﻿using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Bets;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using MatchFixer.Infrastructure.Contracts;
using static MatchFixer.Common.GeneralConstants.BettingServiceConstants;

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
			return (NoBetsProvided, false);

		if (betSlipDto.Amount <= 0)
			return (BetAmountMustBeGreaterThanZero, false);

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

			// Validate if all events in the betlsip are actually live ( started events fall off site and cannot be bet on ) 
			if (DateTime.UtcNow >= matchEvent.MatchDate)
				return (EventAlreadyStartedInSlip, false);

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

		return (BetSlipSubmittedSuccessfully, true);
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
			.Include(bs => bs.Bets)
			.ThenInclude(b => b.MatchEvent)
			.ThenInclude(me => me.LiveResult) 
			.Where(bs => bs.UserId == userId)
			.ToListAsync();


		foreach (var slip in betSlips)
		{
			slip.Bets = slip.Bets
				.Where(b => b.Status != BetStatus.Voided)
				.ToList();

		}

		betSlips = betSlips
			.Where(bs => bs.Bets.Any())
			.ToList();

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
				? (bs.WinAmount > 0 ? nameof(BetStatus.Won) : nameof(BetStatus.Lost))
				: nameof(BetStatus.Pending),

			Bets = bs.Bets.Select(b =>
			{
				var result = b.MatchEvent.LiveResult;
				string? outcome = null;

				if (result != null)
				{
					outcome = result.HomeScore > result.AwayScore
						? MatchPick.Home.ToString()
						: result.HomeScore < result.AwayScore
							? MatchPick.Away.ToString()
							: MatchPick.Draw.ToString();
				}

				return new SingleBetDto
				{
					MatchId = b.MatchEventId,
					HomeTeam = b.MatchEvent.HomeTeam.Name,
					AwayTeam = b.MatchEvent.AwayTeam.Name,
					SelectedOption = b.Pick.ToString(),
					Odds = b.Odds,
					Outcome = outcome
				};
			}).ToList()
		});
	}

	public async Task<bool> CancelBetsForMatchAsync(Guid matchEventId)
	{
		var relatedBets = await _dbContext.Bets
			.Include(b => b.BetSlip)
			.Where(b => b.MatchEventId == matchEventId && b.Status == BetStatus.Pending)
			.ToListAsync();

		if (!relatedBets.Any())
			return true;

		var betSlipGroups = relatedBets.GroupBy(b => b.BetSlipId);

		foreach (var group in betSlipGroups)
		{
			var betSlip = group.First().BetSlip;

			// Void only the bets for the canceled match
			foreach (var bet in group)
				bet.Status = BetStatus.Voided;

			// Load all bets for this slip (regardless of status)
			var allBetsInSlip = await _dbContext.Bets
				.Where(b => b.BetSlipId == betSlip.Id)
				.ToListAsync();

			bool allVoided = allBetsInSlip.All(b => b.Status == BetStatus.Voided);
			bool anyPending = allBetsInSlip.Any(b => b.Status == BetStatus.Pending);
			bool allWon = allBetsInSlip.All(b => b.Status == BetStatus.Won || b.Status == BetStatus.Voided);
			bool anyLost = allBetsInSlip.Any(b => b.Status == BetStatus.Lost);

			if (!anyPending)
			{
				// Settle the slip because no more pending bets
				betSlip.IsSettled = true;

				if (allVoided)
				{
					betSlip.WinAmount = 0;
					await _walletService.RefundBetAsync(betSlip.UserId, betSlip.Amount, betSlip.Id);
				}
				else if (allWon && !anyLost)
				{
					// Calculate winnings from only the WON bets (skip voided ones)
					var totalOdds = allBetsInSlip
						.Where(b => b.Status == BetStatus.Won)
						.Aggregate(1m, (acc, bet) => acc * bet.Odds);

					betSlip.WinAmount = Math.Round(betSlip.Amount * totalOdds, 2);

					await _walletService.AwardWinningsAsync(betSlip.UserId, betSlip.WinAmount.Value, betSlip.Id.ToString());
				}
				else
				{
					// Some bet lost, no win
					betSlip.WinAmount = 0;
				}
			}
		}

		await _dbContext.SaveChangesAsync();
		return true;
	}


	public async Task<bool> EvaluateBetSlipAsync(Guid betSlipId)
	{
		var betSlip = await _dbContext.BetSlips
			.Include(bs => bs.Bets)
			.FirstOrDefaultAsync(bs => bs.Id == betSlipId);

		if (betSlip == null || betSlip.IsSettled)
			return false;

		var bets = betSlip.Bets.ToList();
		bool anyPending = false;
		bool anyLost = false;
		bool allVoided = true;
		decimal totalOdds = 1.0m;

		foreach (var bet in bets)
		{
			if (bet.Status == BetStatus.Voided)
				continue;

			var match = await _dbContext.MatchEvents
				.Include(m => m.LiveResult)
				.FirstOrDefaultAsync(m => m.Id == bet.MatchEventId);

			if (match?.LiveResult == null)
			{
				anyPending = true;
				continue;
			}

			var actualOutcome = match.LiveResult.HomeScore > match.LiveResult.AwayScore
				? MatchPick.Home
				: match.LiveResult.HomeScore < match.LiveResult.AwayScore
					? MatchPick.Away
					: MatchPick.Draw;

			if (bet.Pick == actualOutcome)
			{
				bet.Status = BetStatus.Won;
				totalOdds *= bet.Odds;
				allVoided = false;
			}
			else
			{
				bet.Status = BetStatus.Lost;
				anyLost = true;
				allVoided = false;
			}
		}

		// Settle immediately if lost
		if (anyLost)
		{
			betSlip.IsSettled = true;
			betSlip.WinAmount = 0;
			await _dbContext.SaveChangesAsync();
			return true;
		}

		// If any pending and no losses, wait
		if (anyPending)
		{
			return false;
		}
		
		betSlip.IsSettled = true;

		if (allVoided)
		{
			betSlip.WinAmount = 0;
			await _walletService.RefundBetAsync(betSlip.UserId, betSlip.Amount, betSlip.Id);
		}
		else
		{
			betSlip.WinAmount = Math.Round(betSlip.Amount * totalOdds, 2);
			await _walletService.AwardWinningsAsync(betSlip.UserId, betSlip.WinAmount.Value, betSlip.Id.ToString());
		}

		await _dbContext.SaveChangesAsync();
		return true;
	}





}