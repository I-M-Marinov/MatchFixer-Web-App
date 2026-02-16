using MatchFixer.Infrastructure;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.ViewModels;
using MatchFixer.Infrastructure.Entities; 
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;

using static MatchFixer.Common.Admin.AdminUserServiceConstants;

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

				var walletInfo = await _db.Wallets
					.Where(w => userIds.Contains(w.UserId))
					.GroupBy(w => w.UserId)
					.Select(g => new
					{
						UserId = g.Key,
						Balance = g.Sum(x => x.Balance),
						IsLocked = g.Any(x => x.IsLocked)
					})
					.ToDictionaryAsync(x => x.UserId);

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
						WalletBalance = walletInfo.TryGetValue(u.Id, out var info)
							? (decimal?)info.Balance
							: null,

						IsWalletLocked = walletInfo.TryGetValue(u.Id, out var info2)
						                 && info2.IsLocked,
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
					return (false, CannotLockOwnAccount);

				var user = await _userManager.FindByIdAsync(targetUserId.ToString());
				if (user is null) return (false, UserWasNotFound);

				// Ensure lockout is enabled
				if (!await _userManager.GetLockoutEnabledAsync(user))
				{
					var en = await _userManager.SetLockoutEnabledAsync(user, true);
					if (!en.Succeeded) return (false, FailedToEnableLockForUser);
				}

				// Already locked 
				var currentEnd = await _userManager.GetLockoutEndDateAsync(user);
				if (currentEnd.HasValue && currentEnd.Value > DateTimeOffset.UtcNow)
					return (true, UserLockedUntil(currentEnd));

				// Effectively permanent lock (666 years)
				var end = DateTimeOffset.UtcNow.AddYears(666);
				var res = await _userManager.SetLockoutEndDateAsync(user, end);
				// user.WasDeactivatedByAdmin = true;

				return res.Succeeded
					? (true, UserLocked)
					: (false, FailedToLockUser);


			}

			public async Task<(bool Ok, string Message)> UnlockUserAsync(Guid actorId, Guid targetUserId)
			{
				var user = await _userManager.FindByIdAsync(targetUserId.ToString());
				if (user is null) return (false, UserWasNotFound);

				var res1 = await _userManager.SetLockoutEndDateAsync(user, null);
				if (!res1.Succeeded) return (false, FailedTotoClearLockOut);

				await _userManager.ResetAccessFailedCountAsync(user);
				return (true, UserUnlocked);
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
