using MatchFixer.Core.Contracts;
using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MatchFixer.Infrastructure;


namespace MatchFixer.Core.Services
{
	public class UserContextService : IUserContextService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly MatchFixerDbContext _dbContext;

		public UserContextService(IHttpContextAccessor httpContextAccessor, MatchFixerDbContext dbContext)
		{
			_httpContextAccessor = httpContextAccessor;
			_dbContext = dbContext;
		}

		public Guid GetUserId()
		{
			var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
			return Guid.TryParse(userId, out var guid) ? guid : Guid.Empty;
		}

		public async Task<ApplicationUser?> GetCurrentUserAsync()
		{
			var userId = GetUserId();
			if (userId == Guid.Empty)
				return null;

			return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
		}
	}
}
