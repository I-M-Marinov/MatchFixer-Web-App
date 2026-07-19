namespace MatchFixer.Infrastructure.Contracts
{
	public interface IUpcomingMatchSeederService
	{
		Task SeedUpcomingMatchesAsync();
		Task ReseedForLeagueAsync(int leagueId);
		Task ReseedAllLeaguesAsync();
	}
}
