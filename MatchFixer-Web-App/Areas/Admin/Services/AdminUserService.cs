using MatchFixer.Infrastructure;
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

				var usersQ = _db.Users.AsNoTracking();

				if (!string.IsNullOrWhiteSpace(query))
				{
					var q = query.Trim().ToLower();
					usersQ = usersQ.Where(u =>
						u.Email!.ToLower().Contains(q) ||
						(u.FirstName != null && u.FirstName.ToLower().Contains(q)) ||
						(u.LastName != null && u.LastName.ToLower().Contains(q)));
				}

				// --- STATUS FILTER ---
				var now = DateTimeOffset.UtcNow;
				switch ((status ?? "").ToLowerInvariant())
				{
					case "active":
						usersQ = usersQ.Where(u =>
							u.IsActive &&
							!u.IsDeleted &&
							(!u.LockoutEnd.HasValue || u.LockoutEnd <= now));
						break;

					case "locked":
						usersQ = usersQ.Where(u => u.LockoutEnd.HasValue && u.LockoutEnd > now);
						break;

					case "deleted":
						usersQ = usersQ.Where(u => u.IsDeleted);
						break;

					case "unconfirmed":
						usersQ = usersQ.Where(u => !u.EmailConfirmed);
						break;

				}

				var total = await usersQ.CountAsync();

				var users = await usersQ
					.OrderBy(u => u.Email)
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.Select(u => new
					{
						u.Id,
						u.Email,
						u.FirstName,
						u.LastName,
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
					Status = status, 
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


			public async Task<bool> LockUserAsync(Guid userId)
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null) return false;

				if (!user.LockoutEnabled)
				{
					user.LockoutEnabled = true; // ensure lockout works
				}

				// Lock for 100 years (essentially permanent until manually unlocked)

				user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);

				var result = await _userManager.UpdateAsync(user);
				return result.Succeeded;
			}

			public async Task<bool> UnlockUserAsync(Guid userId)
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user == null) return false;

				user.LockoutEnd = null;

				var result = await _userManager.UpdateAsync(user);
				return result.Succeeded;
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
