using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure.Models.FootballAPI;

using static MatchFixer.Common.ServiceConstants.FootballApiConstants;

namespace MatchFixer.Infrastructure.Services
{
	public class FootballApiService
	{
		private readonly HttpClient _httpClient;
		private readonly MatchFixerDbContext _dbContext;
		private readonly string _apiKey;

		public FootballApiService(HttpClient httpClient, MatchFixerDbContext dbContext, IConfiguration config)
		{
			_httpClient = httpClient;
			_dbContext = dbContext;
			_apiKey = config["FootballApi:Key"]; 
		}

		public async Task FetchAndSaveFixturesAsync()
		{
			var leagueIds = new[] {
			PremierLeagueId,
			LaLigaId,
			BundesligaId,
			SeriaAId
		};

			foreach (var leagueId in leagueIds)
			{
				var url = $"https://v3.football.api-sports.io/fixtures?league={leagueId}&season={Season}";

				var request = new HttpRequestMessage(HttpMethod.Get, url);
				request.Headers.Add("x-apisports-key", _apiKey);

				var response = await _httpClient.SendAsync(request);
				response.EnsureSuccessStatusCode();

				var json = await response.Content.ReadAsStringAsync();
				var fixtures = JsonSerializer.Deserialize<FixtureApiResponse>(json, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				foreach (var match in fixtures.Response)
				{
					if (await _dbContext.MatchResults.AnyAsync(m => m.ApiFixtureId == match.Fixture.Id))
						continue; // Avoid duplicates

					var entity = new MatchResult
					{
						ApiFixtureId = match.Fixture.Id,
						Date = match.Fixture.Date,
						LeagueName = match.League.Name,
						Season = match.League.Season,
						HomeTeam = match.Teams.Home.Name,
						HomeTeamLogo = match.Teams.Home.Logo,
						AwayTeam = match.Teams.Away.Name,
						AwayTeamLogo = match.Teams.Away.Logo,
						HomeScore = match.Goals.Home,
						AwayScore = match.Goals.Away
					};

					_dbContext.MatchResults.Add(entity);
				}

				await _dbContext.SaveChangesAsync();
			}
		}
	}
}
