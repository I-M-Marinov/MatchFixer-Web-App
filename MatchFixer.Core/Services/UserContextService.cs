using MatchFixer.Core.Contracts;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;


namespace MatchFixer.Core.Services
{
	public class UserContextService : IUserContextService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public UserContextService(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public Guid GetUserId()
		{
			var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
			return Guid.TryParse(userId, out var guid) ? guid : Guid.Empty;
		}

	}
}
