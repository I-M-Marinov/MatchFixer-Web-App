using MatchFixer.Common.Enums;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Contracts;
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

			// Helper to compute slip status from bets + slip flags
			 static BetStatus ComputeSlipStatus(global::MatchFixer.Infrastructure.Entities.BetSlip s) 
			{
				if (!s.IsSettled) return BetStatus.Pending;

				if (s.Bets.Any() && s.Bets.All(b => b.Status == BetStatus.Voided))
					return BetStatus.Voided;

				return (s.WinAmount ?? 0m) > 0m ? BetStatus.Won : BetStatus.Lost;
			}

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
				.Include(s => s.Bets)
				.ThenInclude(b => b.MatchEvent)
				.ThenInclude(me => me.HomeTeam)
				.Include(s => s.Bets)
				.ThenInclude(b => b.MatchEvent)
				.ThenInclude(me => me.AwayTeam)
				.FirstOrDefaultAsync(s => s.Id == slipId);

			if (slip == null)
				return null;

			return new AdminBetSlipDetailsViewModel
			{
				Id = slip.Id,

				// Status logic (derived from IsSettled + individual bet outcomes)
				SlipStatus = slip.IsSettled
					? (slip.WinAmount.HasValue && slip.WinAmount > 0 ? BetStatus.Won : BetStatus.Lost)
					: BetStatus.Pending,

				Stake = slip.Amount,
				WinAmount = slip.WinAmount,
				PotentialReturn = slip.Bets.Sum(b => b.Odds) * slip.Amount, 

				Selections = slip.Bets.Select(b => new AdminBetSlipSelectionDto
				{
					MatchName = $"{b.MatchEvent.HomeTeam.Name} vs {b.MatchEvent.AwayTeam.Name}",
					Pick = b.Pick.ToString(),
					Odds = b.Odds,

					Status = b.Status.ToString(),
					StatusBadge =
						b.Status == BetStatus.Won ? "bg-success" :
						b.Status == BetStatus.Lost ? "bg-danger" :
						"bg-warning text-dark"
				}).ToList()
			};
		}


	}
}
