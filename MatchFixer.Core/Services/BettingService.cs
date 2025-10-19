using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Bets;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using static MatchFixer.Common.GeneralConstants.BettingServiceConstants;

namespace MatchFixer.Core.Services;

public class BettingService : IBettingService
{
	private readonly MatchFixerDbContext _dbContext;
	private readonly ISessionService _sessionService;
	private readonly ITimezoneService _timezoneService;
	private readonly IWalletService _walletService;
	private readonly ITrophyService _trophyService;
	private readonly IOddsBoostService _oddsBoostService;


	public BettingService(MatchFixerDbContext dbContext, 
		ISessionService sessionService, 
		ITimezoneService timezoneService,
		IWalletService walletService,
		ITrophyService trophyService,
		IOddsBoostService oddsBoostService)
	{
		_dbContext = dbContext;
		_sessionService = sessionService;
		_timezoneService = timezoneService;
		_walletService = walletService;
		_trophyService = trophyService;
		_oddsBoostService = oddsBoostService;

	}

	public async Task<(string Message, bool IsSuccess)> PlaceBetAsync(Guid userId, BetSlipDto betSlipDto, string profileUrl)
	{
		if (betSlipDto == null || betSlipDto.Bets == null || !betSlipDto.Bets.Any())
			return (NoBetsProvided, false);

		if (betSlipDto.Amount <= 0)
			return (BetAmountMustBeGreaterThanZero, false);

		var betSlip = new BetSlip
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			Amount = betSlipDto.Amount,
			BetTime = DateTime.UtcNow,
			IsSettled = false
		};

		// Deduct once for the whole slip
		var (success, message) = await _walletService.DeductForBetAsync(userId, betSlipDto.Amount);
		if (!success)
			return (message, false);

		foreach (var betDto in betSlipDto.Bets)
		{
			var matchEvent = await _dbContext.MatchEvents
				.AsTracking()
				.FirstOrDefaultAsync(x => x.Id == betDto.MatchId); 
			
			if (matchEvent == null)
				return (InvalidMatchId(betDto.MatchId), false);

			if (!IsEventOpenForBetting(matchEvent))
				return (EventOrEventsCancelledInSlip, false);

			if (DateTime.UtcNow >= matchEvent.MatchDate)
				return (EventAlreadyStartedInSlip, false);

			if (!Enum.TryParse<MatchPick>(betDto.SelectedOption, true, out var parsedPick))
				return (InvalidPickOption(betDto.SelectedOption), false);

			// Use the shared odds calculator
			var (home, draw, away, boost) = await _oddsBoostService.GetEffectiveOddsAsync(
				matchEvent.Id,
				matchEvent.HomeOdds,
				matchEvent.DrawOdds,
				matchEvent.AwayOdds
			);

			decimal? effectiveOdds = betDto.SelectedOption.ToLower() switch
			{
				"home" => home,
				"draw" => draw,
				"away" => away,
				_ => betDto.Odds // fallback, shouldn't happen if validated
			};

			if (boost != null)
			{
				// Enforce max stake
				if (boost.MaxStakePerBet.HasValue && betSlipDto.Amount > boost.MaxStakePerBet.Value)
					return (MaxStakePerBetIs(boost.MaxStakePerBet.Value), false);

				// Enforce max uses per user
				if (boost.MaxUsesPerUser.HasValue)
				{
					var used = await _dbContext.Bets
						.CountAsync(x => x.BetSlip.UserId == userId &&
										 x.MatchEventId == matchEvent.Id &&
										 x.OddsBoostId == boost.Id);

					if (used >= boost.MaxUsesPerUser.Value)
						return (BoostAlreadyUsedMaximumTimesForEvent, false);
				}
			}

			var bet = new Bet
			{
				Id = Guid.NewGuid(),
				BetSlipId = betSlip.Id,
				MatchEventId = matchEvent.Id,
				Pick = parsedPick,
				Odds = (decimal)effectiveOdds,
				BetTime = DateTime.UtcNow,
				OddsBoostId = boost?.Id
			};

			betSlip.Bets.Add(bet);
		}

		await _dbContext.BetSlips.AddAsync(betSlip);
		await _dbContext.SaveChangesAsync();

		await _trophyService.EvaluateTrophiesAsync(userId, profileUrl);

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
			.OrderByDescending(b => b.BetTime)
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


	private static bool IsEventOpenForBetting(MatchEvent ev)
	{
		var nowUtc = DateTime.UtcNow;

		// if event was not found return false
		if (ev == null) return false;

		// if event is cancelled return false
		if (ev.IsCancelled) return false;

		// if that event already has a result recorded in the database return false
		if (ev.LiveResult != null) return false;

		return true;
	}


}