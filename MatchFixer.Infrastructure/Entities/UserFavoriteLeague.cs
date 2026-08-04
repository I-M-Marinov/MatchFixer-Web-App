namespace MatchFixer.Infrastructure.Entities
{
	public class UserFavoriteLeague
	{
		public Guid UserId { get; set; }
		public string LeagueName { get; set; } = null!;

		public ApplicationUser User { get; set; } = null!;
	}
}
