using MatchFixer.Common.Enums;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Bets;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer_Web_App.Areas.Admin.Services
{
	public class AdminUserBetsService : IAdminUserBetsService
	{
		private readonly MatchFixerDbContext _dbContext;
		private readonly ITimezoneService _timezoneService;

		public AdminUserBetsService(MatchFixerDbContext dbContext, ITimezoneService timezoneService)
		{
			_dbContext = dbContext;
			_timezoneService = timezoneService;
		}

		public async Task<AdminUserBetsColumnsViewModel?> GetColumnsAsync(Guid userId, string timeZoneId, int maxPerColumn = 200)
		{
			// Header info
			var user = await _dbContext.Users.AsNoTracking()
				.Where(u => u.Id == userId)
				.Select(u => new { u.UserName, u.Email })
				.FirstOrDefaultAsync();

			if (user is null) return null;

			// Load slips 
			var slips = await _dbContext.BetSlips
				.AsNoTracking()
				.Where(s => s.UserId == userId)
				.Include(s => s.Bets)
				.OrderByDescending(s => s.BetTime)
				.ToListAsync();


			// Map to admin rows
			var rows = slips.Select(s =>
			{
				decimal totalOdds = 1m;

				foreach (var b in s.Bets.Where(b => b.Status != BetStatus.Voided))
					totalOdds *= b.Odds;

				decimal? potential = s.Bets.Count == 0 ? null : Math.Round(s.Amount * totalOdds, 2);

				var createdDisplay = _timezoneService.FormatForUserExact(s.BetTime, timeZoneId, "dd.MM.yyyy HH:mm");
				string? settledDisplay = null;

				var status = ComputeSlipStatus(s);

				return new AdminBetSlipRowDto
				{
					Id = s.Id,
					SlipStatus = status,
					Stake = s.Amount,
					WinAmount = s.WinAmount,
					PotentialReturn = potential,
					Selections = s.Bets.Count,
					CreatedUtc = DateTime.SpecifyKind(s.BetTime, DateTimeKind.Utc),
					CreatedDisplay = createdDisplay,
					SettledUtc = null,
					SettledDisplay = settledDisplay
				};
			}).ToList();

			var pending = rows.Where(r => r.SlipStatus == BetStatus.Pending).Take(maxPerColumn).ToList();
			var won = rows.Where(r => r.SlipStatus == BetStatus.Won).Take(maxPerColumn).ToList();
			var lostOrVoided = rows.Where(r => r.SlipStatus == BetStatus.Lost || r.SlipStatus == BetStatus.Voided)
								   .Take(maxPerColumn).ToList();

			return new AdminUserBetsColumnsViewModel
			{
				UserId = userId,
				UserName = user.UserName,
				Email = user.Email,
				Pending = pending,
				Won = won,
				LostOrVoided = lostOrVoided
			};
		}

		public async Task<AdminBetSlipDetailsViewModel?> GetSlipDetailsAsync(Guid slipId)
		{
			var slip = await _dbContext.BetSlips
				.AsNoTracking()
				.Include(s => s.Bets)
					.ThenInclude(b => b.MatchEvent)
						.ThenInclude(me => me.HomeTeam)
				.Include(s => s.Bets)
					.ThenInclude(b => b.MatchEvent)
						.ThenInclude(me => me.AwayTeam)
				.Include(s => s.Bets)
					.ThenInclude(b => b.MatchEvent)
						.ThenInclude(me => me.LiveResult)
				.FirstOrDefaultAsync(s => s.Id == slipId);

			if (slip == null)
				return null;

			/* -----------------------------
			 *  CALCULATE TOTAL ODDS (READ ONLY)
			 * ----------------------------- */

			decimal totalOdds = slip.Bets
				.Where(b => b.Status != BetStatus.Voided)
				.Aggregate(1m, (acc, b) => acc * b.Odds);

			decimal potentialReturn = slip.Amount * totalOdds;

			/* -----------------------------
			 *  DETERMINE SLIP STATUS (PURE)
			 * ----------------------------- */

			var slipStatus = ComputeSlipStatus(slip);

			decimal? winAmount = slipStatus switch
			{
				BetStatus.Won => potentialReturn,
				BetStatus.Voided => slip.Amount,
				_ => 0m
			};

			/* -----------------------------
			 *  MAP VIEW MODEL
			 * ----------------------------- */

			return new AdminBetSlipDetailsViewModel
			{
				Id = slip.Id,
				SlipStatus = slipStatus,

				Stake = slip.Amount,
				WinAmount = winAmount,
				PotentialReturn = potentialReturn,

				Selections = slip.Bets.Select(b => new AdminBetSlipSelectionDto
				{
					MatchName = $"{b.MatchEvent.HomeTeam.Name} vs {b.MatchEvent.AwayTeam.Name}",
					Pick = b.Pick.ToString(),
					Odds = b.Odds,
					HomeTeamLogoUrl = b.MatchEvent.HomeTeam.LogoUrl,
					AwayTeamLogoUrl = b.MatchEvent.AwayTeam.LogoUrl,
					Status = b.Status.ToString(),
					StatusBadge = b.Status switch
					{
						BetStatus.Won => "bg-success",
						BetStatus.Lost => "bg-danger",
						BetStatus.Voided => "bg-secondary text-white",
						BetStatus.Pending => "bg-warning text-dark",
						_ => "bg-light text-dark"
					}
				}).ToList()
			};
		}


		// Helper to compute slip status from bets + slip flags
		static BetStatus ComputeSlipStatus(BetSlip s)
		{
			if (s.Bets.Any(b => b.Status == BetStatus.Voided))
				return BetStatus.Voided;

			if (s.Bets.Any(b => b.Status == BetStatus.Lost))
				return BetStatus.Lost;

			if (s.Bets.Any(b => b.Status == BetStatus.Pending))
				return BetStatus.Pending;

			return BetStatus.Won;
		}



	}
}
