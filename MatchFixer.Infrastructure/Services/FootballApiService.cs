using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure.Models.FootballAPI;

using static MatchFixer.Common.ServiceConstants.FootballApiConstants;
using System.Globalization;

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

					// Ensure both teams exist in database
					await EnsureTeamExistsAsync(match.Teams.Home.Name, match.Teams.Home.Logo);
					await EnsureTeamExistsAsync(match.Teams.Away.Name, match.Teams.Away.Logo);

					// Get teams from the database by name 
					var homeTeam = await _dbContext.Teams
						.FirstOrDefaultAsync(t => t.Name == match.Teams.Home.Name);
					var awayTeam = await _dbContext.Teams
						.FirstOrDefaultAsync(t => t.Name == match.Teams.Away.Name);

					// if finding them by name fails, look for them by TeamId ( id that matches the Football API Id's for the team logos ) 
					if (homeTeam == null)
					{
						int? homeTeamId = ExtractTeamIdFromLogoUrl(match.Teams.Home.Logo);

						homeTeam = await _dbContext.Teams
							.FirstOrDefaultAsync(t => t.TeamId == homeTeamId);
					}

					if (awayTeam == null)
					{
						int? awayTeamId = ExtractTeamIdFromLogoUrl(match.Teams.Away.Logo);

						awayTeam = await _dbContext.Teams
							.FirstOrDefaultAsync(t => t.TeamId == awayTeamId);
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
				await _dbContext.SaveChangesAsync(); // Save all additions to the DB
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
					// Avoid duplicates by checking the TeamId
					int? teamId = ExtractTeamIdFromLogoUrl(teamInfo.Team.Logo);

					if (teamId.HasValue)
					{
						bool exists = await _dbContext.Teams.AnyAsync(t => t.TeamId == teamId.Value);

						if (!exists)
						{
							var team = new Team
							{
								Id = Guid.NewGuid(),
								TeamId = teamId,
								Name = teamInfo.Team.Name,
								LogoUrl = teamInfo.Team.Logo
							};

							await _dbContext.Teams.AddAsync(team);
						}
					}
				}
			}

			await _dbContext.SaveChangesAsync();
		}

		// Helper method to extract team ID
		private int? ExtractTeamIdFromLogoUrl(string logoUrl)
		{
			if (string.IsNullOrEmpty(logoUrl))
				return null;

			try
			{
				var uri = new Uri(logoUrl);
				var fileName = Path.GetFileNameWithoutExtension(uri.AbsolutePath); // Gets the team id from the uri

				if (int.TryParse(fileName, out int teamId))
				{
					return teamId;
				}

				return null;
			}
			catch
			{
				return null;
			}
		}

		// Helper method to check if the Team we are trying to use for the MatchResult is actually in the DB ( if not it adds it )
		private async Task EnsureTeamExistsAsync(string teamName, string logoUrl)
		{
			int? teamId = ExtractTeamIdFromLogoUrl(logoUrl);

			if (!teamId.HasValue)
				return;

			bool exists = await _dbContext.Teams.AnyAsync(t => t.TeamId == teamId.Value);

			if (!exists)
			{
				var team = new Team
				{
					Id = Guid.NewGuid(),
					TeamId = teamId.Value,
					Name = teamName,
					LogoUrl = logoUrl
				};

				await _dbContext.Teams.AddAsync(team);
				await _dbContext.SaveChangesAsync();
			}
		}

	}
}
