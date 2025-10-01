﻿using MatchFixer.Infrastructure;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.ViewModels;
using MatchFixer.Infrastructure.Entities; 
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace MatchFixer_Web_App.Areas.Admin.Services
{

	

	namespace MatchFixer_Web_App.Areas.Admin.Services
	{
		public class AdminUserService : IAdminUserService
		{
			private readonly MatchFixerDbContext _db;
			private readonly UserManager<ApplicationUser> _userManager;
			private readonly RoleManager<IdentityRole<Guid>> _roleManager;

			public AdminUserService(
				MatchFixerDbContext db,
				UserManager<ApplicationUser> userManager,
				RoleManager<IdentityRole<Guid>> roleManager)
			{
				_db = db;
				_userManager = userManager;
				_roleManager = roleManager;
			}

			public async Task<AdminUsersListViewModel> GetUsersAsync(string? query, string? status, int page, int pageSize)
			{
				if (page <= 0) page = 1;
				if (pageSize <= 0 || pageSize > 200) pageSize = 20;

				status = string.IsNullOrWhiteSpace(status) ? "active" : status.ToLowerInvariant();

				var usersQ = _db.Users.AsNoTracking();

				if (!string.IsNullOrWhiteSpace(query))
				{
					var q = query.Trim().ToLower();
					usersQ = usersQ.Where(u =>
						(u.Email ?? "").ToLower().Contains(q) ||
						((u.FirstName ?? "").ToLower().Contains(q)) ||
						((u.LastName ?? "").ToLower().Contains(q)));
				}

				// --- STATUS FILTER ---
				var now = DateTimeOffset.UtcNow;
				usersQ = status switch
				{
					"active" => usersQ.Where(u =>
						u.IsActive &&
						!u.IsDeleted &&
						(!u.LockoutEnd.HasValue || u.LockoutEnd <= now)),

					"unconfirmed" => usersQ.Where(u => !u.EmailConfirmed && !u.IsDeleted),

					"locked" => usersQ.Where(u => u.LockoutEnd.HasValue && u.LockoutEnd > now && !u.IsDeleted),

					"deleted" => usersQ.Where(u => u.IsDeleted),

					"all" or _ => usersQ.Where(u => !u.IsDeleted) // fallback
				};

				var total = await usersQ.CountAsync();

				var users = await usersQ
					.Include(u => u.ProfilePicture)
					.OrderBy(u => u.Email)
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.Select(u => new
					{
						u.Id,
						u.Email,
						u.FirstName,
						u.LastName,
						u.ProfilePicture,
						u.EmailConfirmed,
						u.LockoutEnd,
						u.LockoutEnabled,
						u.IsActive,
						u.IsDeleted
					})
					.ToListAsync();

				var userIds = users.Select(u => u.Id).ToList();

				var rolesMap = new Dictionary<Guid, string[]>();
				foreach (var u in users)
				{
					var user = await _userManager.FindByIdAsync(u.Id.ToString());
					var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
					rolesMap[u.Id] = roles.ToArray();
				}

				var walletBalances = await _db.Wallets
					.Where(w => userIds.Contains(w.UserId))
					.GroupBy(w => w.UserId)
					.Select(g => new { UserId = g.Key, Balance = g.Sum(x => x.Balance) })
					.ToDictionaryAsync(x => x.UserId, x => (decimal?)x.Balance);

				var betsCount = await _db.Bets
					.Where(b => userIds.Contains(b.BetSlip.UserId))
					.GroupBy(b => b.BetSlip.UserId)
					.Select(g => new { UserId = g.Key, Count = g.Count() })
					.ToDictionaryAsync(x => x.UserId, x => x.Count);

				var vm = new AdminUsersListViewModel
				{
					Query = query,
					Status = status, // add the active status to the model
					Page = page,
					PageSize = pageSize,
					Total = total,
					Users = users.Select(u => new AdminUserRow
					{
						Id = u.Id,
						Email = u.Email ?? "",
						FirstName = u.FirstName,
						LastName = u.LastName,
						EmailConfirmed = u.EmailConfirmed,
						UserImage = string.IsNullOrWhiteSpace(u.ProfilePicture?.ImageUrl) ? "N/A" : u.ProfilePicture.ImageUrl,
						IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd.Value > now,
						LockoutEnd = u.LockoutEnd,
						Roles = rolesMap.TryGetValue(u.Id, out var r) ? r : Array.Empty<string>(),
						WalletBalance = walletBalances.TryGetValue(u.Id, out var bal) ? bal : null,
						BetsCount = betsCount.TryGetValue(u.Id, out var cnt) ? cnt : 0,
						IsActive = u.IsActive,
						IsDeleted = u.IsDeleted
					}).ToList()
				};

				return vm;
			}



			public async Task<(bool Ok, string Message)> LockUserAsync(Guid actorId, Guid targetUserId)
			{
				if (actorId == targetUserId)
					return (false, "You cannot lock your own account.");

				var user = await _userManager.FindByIdAsync(targetUserId.ToString());
				if (user is null) return (false, "User not found.");

				// Ensure lockout is enabled
				if (!await _userManager.GetLockoutEnabledAsync(user))
				{
					var en = await _userManager.SetLockoutEnabledAsync(user, true);
					if (!en.Succeeded) return (false, "Failed to enable lockout for this user.");
				}

				// Already locked 
				var currentEnd = await _userManager.GetLockoutEndDateAsync(user);
				if (currentEnd.HasValue && currentEnd.Value > DateTimeOffset.UtcNow)
					return (true, $"User is already locked until {currentEnd.Value:yyyy-MM-dd HH:mm} UTC.");

				// Effectively permanent lock (666 years)
				var end = DateTimeOffset.UtcNow.AddYears(666);
				var res = await _userManager.SetLockoutEndDateAsync(user, end);
				return res.Succeeded
					? (true, "User locked.")
					: (false, "Failed to lock user.");
			}

			public async Task<(bool Ok, string Message)> UnlockUserAsync(Guid actorId, Guid targetUserId)
			{
				var user = await _userManager.FindByIdAsync(targetUserId.ToString());
				if (user is null) return (false, "User not found.");

				var res1 = await _userManager.SetLockoutEndDateAsync(user, null);
				if (!res1.Succeeded) return (false, "Failed to clear lockout end date.");

				await _userManager.ResetAccessFailedCountAsync(user);
				return (true, "User unlocked.");
			}

			public async Task<bool> MarkEmailConfirmedAsync(Guid userId)
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null) return false;

				if (user.EmailConfirmed) return true;

				user.EmailConfirmed = true;
				var result = await _userManager.UpdateAsync(user);
				return result.Succeeded;
			}

			public async Task<(bool ok, string? resetLink)> GenerateResetPasswordLinkAsync(Guid userId, IUrlHelper url)
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null || user.Email == null) return (false, null);

				var token = await _userManager.GeneratePasswordResetTokenAsync(user);

				var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

				var link = url.Page(
					pageName: "/Account/ResetPassword",
					pageHandler: null,
					values: new { area = "Identity", code = encoded, userId = user.Id },
					protocol: url.ActionContext.HttpContext.Request.Scheme);

				return (true, link);
			}

			public async Task<bool> AddRoleAsync(Guid userId, string role)
			{
				if (string.IsNullOrWhiteSpace(role)) return false;

				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null) return false;

				if (!await _roleManager.RoleExistsAsync(role))
				{
					var create = await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
					if (!create.Succeeded) return false;
				}

				if (await _userManager.IsInRoleAsync(user, role)) return true;

				var result = await _userManager.AddToRoleAsync(user, role);
				return result.Succeeded;
			}

			public async Task<bool> RemoveRoleAsync(Guid userId, string role)
			{
				if (string.IsNullOrWhiteSpace(role)) return false;

				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null) return false;

				if (!await _userManager.IsInRoleAsync(user, role)) return true;

				var result = await _userManager.RemoveFromRoleAsync(user, role);
				return result.Succeeded;
			}
		}
	}
}
