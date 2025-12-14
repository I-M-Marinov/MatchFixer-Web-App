using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.UserTrophyContext;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer.Core.Services.Admin
{
	public class AdminTrophyService : IAdminTrophyService
	{
		private readonly MatchFixerDbContext _dbContext;
		private readonly ITrophyService _trophyService;

		public AdminTrophyService(
			MatchFixerDbContext dbContext,
			ITrophyService trophyService)
		{
			_dbContext = dbContext;
			_trophyService = trophyService;
		}

		public async Task ReevaluateUserTrophiesAsync(Guid userId)
		{
			var allTrophies = await _dbContext.Trophies.AsNoTracking().ToListAsync();

			var currentUserTrophies = await _dbContext.UserTrophies
				.Where(ut => ut.UserId == userId)
				.ToListAsync();

			var validTrophyIds = await EvaluateValidTrophyIdsAsync(userId);

			var toRemove = currentUserTrophies
				.Where(ut => !validTrophyIds.Contains(ut.TrophyId))
				.ToList();

			if (toRemove.Any())
			{
				_dbContext.UserTrophies.RemoveRange(toRemove);
			}

			await _dbContext.SaveChangesAsync();

			foreach (var trophyId in validTrophyIds)
			{
				bool alreadyHas = currentUserTrophies.Any(ut => ut.TrophyId == trophyId);
				if (!alreadyHas)
				{
					await _trophyService.EvaluateTrophiesAsync(userId);
					break; // Evaluate once, service handles the rest
				}
			}
		}

		public async Task ReevaluateAllUsersAsync()
		{
			var userIds = await _dbContext.Users
				.AsNoTracking()
				.Select(u => u.Id)
				.ToListAsync();

			foreach (var userId in userIds)
			{
				await ReevaluateUserTrophiesAsync(userId);
			}
		}


		// Returns the set of trophy IDs the user SHOULD currently have

		private async Task<HashSet<int>> EvaluateValidTrophyIdsAsync(Guid userId)
		{
			var now = DateTime.UtcNow;

			var trophies = await _dbContext.Trophies.AsNoTracking().ToListAsync();

			var userBets = await _dbContext.Bets
				.Include(b => b.BetSlip)
				.Include(b => b.MatchEvent)
				.Where(b => b.BetSlip.UserId == userId)
				.OrderBy(b => b.BetTime)
				.ToListAsync();

			var user = await _dbContext.Users
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Id == userId);

			if (user == null)
				return new HashSet<int>();

			var context = new UserTrophyContext
			{
				UserId = userId,
				User = user,
				UserBets = userBets,
				TotalBets = userBets.Count,
				TotalWagered = userBets
					.Select(b => b.BetSlip)
					.Distinct()
					.Sum(s => s.Amount),
				Now = now,
				DbContext = _dbContext
			};

			var result = new HashSet<int>();

			foreach (var trophy in trophies)
			{
				if (_trophyService is TrophyService svc &&
					svc.TryGetCondition(trophy.Name, out var condition))
				{
					if (await condition(context))
					{
						result.Add(trophy.Id);
					}
				}
			}

			return result;
		}

	}

}
