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
			var leagueIds = new[]
			{
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
						continue;

					// Get or create Home Team
					var homeTeam = await _dbContext.Teams
						.FirstOrDefaultAsync(t => t.Name == match.Teams.Home.Name);

					if (homeTeam == null)
					{
						homeTeam = new Team
						{
							Id = Guid.NewGuid(),
							Name = match.Teams.Home.Name,
							LogoUrl = match.Teams.Home.Logo
						};

						await _dbContext.Teams.AddAsync(homeTeam);
						await _dbContext.SaveChangesAsync(); // Save to get ID for FK
					}

					// Get or create Away Team
					var awayTeam = await _dbContext.Teams
						.FirstOrDefaultAsync(t => t.Name == match.Teams.Away.Name);

					if (awayTeam == null)
					{
						awayTeam = new Team
						{
							Id = Guid.NewGuid(),
							Name = match.Teams.Away.Name,
							LogoUrl = match.Teams.Away.Logo
						};

						await _dbContext.Teams.AddAsync(awayTeam);
						await _dbContext.SaveChangesAsync();
					}

					// Create MatchResult
					var entity = new MatchResult
					{
						ApiFixtureId = match.Fixture.Id,
						Date = match.Fixture.Date,
						LeagueName = match.League.Name,
						Season = match.League.Season,

						HomeTeamId = homeTeam.Id,
						AwayTeamId = awayTeam.Id,

						HomeScore = match.Goals.Home,
						AwayScore = match.Goals.Away
					};

					await _dbContext.MatchResults.AddAsync(entity);
				}

				await _dbContext.SaveChangesAsync();
			}
		}


		public async Task FetchAndSaveTeamsAsync()
		{
			var leagueIds = new[]
			{
				PremierLeagueId,
				LaLigaId,
				BundesligaId,
				SeriaAId,
				Ligue1Id,
				EredivisieId,
				LigaPortugalId,
				CroatianLeagueId,
				SwissLeagueId
			};

			foreach (var leagueId in leagueIds)
			{
				var url = $"https://v3.football.api-sports.io/teams?league={leagueId}&season={Season}";

				var request = new HttpRequestMessage(HttpMethod.Get, url);
				request.Headers.Add("x-apisports-key", _apiKey);

				var response = await _httpClient.SendAsync(request);
				response.EnsureSuccessStatusCode();

				var json = await response.Content.ReadAsStringAsync();
				var apiResponse = JsonSerializer.Deserialize<TeamListApiResponse>(json, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				foreach (var teamInfo in apiResponse.Response)
				{
					// Avoid duplicates by name 
					bool exists = await _dbContext.Teams.AnyAsync(t => t.Name == teamInfo.Team.Name);

					if (!exists)
					{
						var team = new Team
						{
							Id = Guid.NewGuid(),
							Name = teamInfo.Team.Name,
							LogoUrl = teamInfo.Team.Logo
						};

						await _dbContext.Teams.AddAsync(team);
					}
				}
			}

			await _dbContext.SaveChangesAsync();
		}

	}
}
