using MatchFixer_Web_App.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MatchFixer_Web_App.Areas.Admin.Interfaces
{
	public interface IAdminUserService
	{
		Task<AdminUsersListViewModel> GetUsersAsync(string? query, int page, int pageSize);

		Task<bool> LockUserAsync(Guid userId);
		Task<bool> UnlockUserAsync(Guid userId);
		Task<bool> MarkEmailConfirmedAsync(Guid userId);

		Task<(bool ok, string? resetLink)> GenerateResetPasswordLinkAsync(Guid userId, IUrlHelper url);
		Task<bool> AddRoleAsync(Guid userId, string role);
		Task<bool> RemoveRoleAsync(Guid userId, string role);
	}
}
