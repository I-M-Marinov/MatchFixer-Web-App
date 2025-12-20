namespace MatchFixer.Infrastructure.Contracts
{
	public interface IUpcomingMatchSeederService
	{
		Task SeedUpcomingMatchesAsync();
	}
}
