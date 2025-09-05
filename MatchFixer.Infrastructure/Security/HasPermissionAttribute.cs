using Microsoft.AspNetCore.Authorization;

namespace MatchFixer.Infrastructure.Security
{
	public class HasPermissionAttribute : AuthorizeAttribute
	{
		public HasPermissionAttribute(string permission)
		{
			Policy = permission; // uses the policies you registered in Program.cs
		}
	}
}