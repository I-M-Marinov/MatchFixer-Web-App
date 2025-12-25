using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

using static MatchFixer.Common.FootballLeagues.SupportedApiLeagues;

namespace MatchFixer.Infrastructure.Services
{
	public class UpcomingMatchSeederService : IUpcomingMatchSeederService
	{
		private readonly MatchFixerDbContext _dbContext;
		private readonly IFootballApiService _footballApiService;

		public UpcomingMatchSeederService(
			MatchFixerDbContext dbContext,
			IFootballApiService footballApiService)
		{
			_dbContext = dbContext;
			_footballApiService = footballApiService;
		}

		public async Task SeedUpcomingMatchesAsync()
		{
			foreach (var league in Football.Keys)
			{
				Console.WriteLine($"[UpcomingSeeder] Fetching upcoming matches for league {league}");

				var upcomingFromApi =
					await _footballApiService.GetUpcomingFromApiAsync(league);

				if (!upcomingFromApi.Any())
					continue;

				foreach (var apiMatch in upcomingFromApi)
				{
					// 🔐 HARD DEDUPLICATION
					bool exists = await _dbContext.UpcomingMatchEvents
						.AnyAsync(x => x.ApiFixtureId == apiMatch.ApiFixtureId);

					if (exists)
						continue;

					var homeTeam = await ResolveTeamAsync(
						apiMatch.HomeName,
						apiMatch.HomeLogo);

					var awayTeam = await ResolveTeamAsync(
						apiMatch.AwayName,
						apiMatch.AwayLogo);

					if (homeTeam == null || awayTeam == null)
					{
						Console.WriteLine(
							$"[UpcomingSeeder] Skipped fixture {apiMatch.ApiFixtureId} (team missing)");
						continue;
					}

					var entity = new UpcomingMatchEvent
					{
						Id = Guid.NewGuid(),
						ApiFixtureId = apiMatch.ApiFixtureId,
						ApiLeagueId = league,
						MatchDateUtc = apiMatch.KickoffUtc.UtcDateTime,
						HomeTeamId = homeTeam.Id,
						AwayTeamId = awayTeam.Id,
						IsCancelled = false,
						IsImported = false,
						ImportedAtUtc = DateTime.UtcNow
					};

					await _dbContext.UpcomingMatchEvents.AddAsync(entity);
				}

				await _dbContext.SaveChangesAsync();

				// API LIMIT: 1 request every 1 seconds ( free version 1 request every 6 seconds ) 
				await Task.Delay(1000);
			}
		}

		// =========================
		// Helpers (copied logic)
		// =========================

		private async Task<Team?> ResolveTeamAsync(string teamName, string logoUrl)
		{
			// Try by API Team ID
			var apiTeamId = ExtractTeamIdFromLogoUrl(logoUrl);

			if (apiTeamId.HasValue)
			{
				var byId = await _dbContext.Teams
					.AsNoTracking()
					.FirstOrDefaultAsync(t => t.TeamId == apiTeamId.Value);
				
				if (byId != null)
					return byId;
			}

			// Fallback by name
			return await _dbContext.Teams
				.AsNoTracking()
				.FirstOrDefaultAsync(t => t.Name == teamName);
		}

		private int? ExtractTeamIdFromLogoUrl(string logoUrl)
		{
			if (string.IsNullOrWhiteSpace(logoUrl))
				return null;

			try
			{
				var uri = new Uri(logoUrl);
				var fileName = Path.GetFileNameWithoutExtension(uri.AbsolutePath);

				return int.TryParse(fileName, out var teamId)
					? teamId
					: null;
			}
			catch
			{
				return null;
			}
		}
	}
}
