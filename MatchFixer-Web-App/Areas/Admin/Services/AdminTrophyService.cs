using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.UserTrophyContext;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Trophy;
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
			var current = await _dbContext.UserTrophies
				.Where(ut => ut.UserId == userId)
				.ToListAsync();

			var valid = await EvaluateValidTrophyIdsAsync(userId);

			var toRemove = current
				.Where(ut => !valid.Contains(ut.TrophyId))
				.ToList();

			if (toRemove.Any())
				_dbContext.UserTrophies.RemoveRange(toRemove);
			var existingIds = current.Select(ut => ut.TrophyId).ToHashSet();

			var toAdd = valid.Except(existingIds);

			foreach (var trophyId in toAdd)
			{
				_dbContext.UserTrophies.Add(new UserTrophy
				{
					UserId = userId,
					TrophyId = trophyId,
					AwardedOn = DateTime.UtcNow,
					IsNew = false // admin operation
				});
			}

			await _dbContext.SaveChangesAsync();
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


			var relevantBets = userBets
				.Where(b => b.Status != BetStatus.Voided)
				.ToList();

			var context = new UserTrophyContext
			{
				UserId = userId,
				User = user,
				UserBets = relevantBets,
				TotalBets = relevantBets.Count,
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

		public async Task<AdminUserTrophyViewModel?> GetUserWithTrophiesAsync(Guid userId)
		{
			return await _dbContext.Users
				.AsNoTracking()
				.Where(u => u.Id == userId)
				.Select(u => new AdminUserTrophyViewModel
				{
					UserId = u.Id,
					Email = u.Email,
					FullName = u.FirstName + " " + u.LastName,
					Trophies = u.UserTrophies
						.Select(ut => new AdminTrophyItemViewModel
						{
							TrophyId = ut.TrophyId,
							Name = ut.Trophy.Name,
							IconUrl = ut.Trophy.IconUrl,
							Description = ut.Trophy.Description,
							Level = ut.Trophy.Level,
							AwardedOn = ut.AwardedOn
						})
						.OrderByDescending(t => t.AwardedOn)
						.ToList()
				})
				.FirstOrDefaultAsync();
		}


		public async Task<List<AdminUserTrophyViewModel>> GetUsersWithTrophiesAsync()
		{
			return await _dbContext.Users
				.Where(u => u.IsActive)
				.AsNoTracking()
				.Select(u => new AdminUserTrophyViewModel
				{
					UserId = u.Id,
					Email = u.Email,
					FullName = u.FirstName + " " + u.LastName,
					Trophies = u.UserTrophies
						.Select(ut => new AdminTrophyItemViewModel
						{
							TrophyId = ut.TrophyId,
							Name = ut.Trophy.Name,
							IconUrl = ut.Trophy.IconUrl,
							Description = ut.Trophy.Description,
							Level = ut.Trophy.Level,
							AwardedOn = ut.AwardedOn
						})
						.OrderByDescending(t => t.AwardedOn)
						.ToList()
				})
				.OrderBy(u => u.Email)
				.ToListAsync();
		}


	}

}
