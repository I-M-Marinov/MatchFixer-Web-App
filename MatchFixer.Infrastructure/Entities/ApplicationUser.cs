using Microsoft.AspNetCore.Identity;


namespace MatchFixer.Infrastructure.Entities
{
	public class ApplicationUser : IdentityUser<Guid>
	{
		public ApplicationUser() { }

		public ICollection<Bet> Bets { get; set; } = new List<Bet>();
	}
}
