namespace MatchFixer.Infrastructure.Entities
{
	public class UserFavoriteTeam
	{
		public Guid UserId { get; set; }
		public Guid TeamId { get; set; }

		public ApplicationUser User { get; set; }
		public Team Team { get; set; }
	}
}
