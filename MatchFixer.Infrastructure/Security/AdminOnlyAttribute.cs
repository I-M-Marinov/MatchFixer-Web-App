using Microsoft.AspNetCore.Authorization;
using static MatchFixer.Common.Identity.Roles; 

namespace MatchFixer.Infrastructure.Security
{
	public sealed class AdminOnlyAttribute : AuthorizeAttribute
	{
		public AdminOnlyAttribute()
		{
			Roles = Admin;
		}
	}
}