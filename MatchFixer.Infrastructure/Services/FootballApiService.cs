using System.Runtime.CompilerServices;
using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure.Models.FootballAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using MatchFixer.Common.FootballLeagues;
using static MatchFixer.Common.ServiceConstants.FootballApiConstants;

namespace MatchFixer.Infrastructure.Services
{
	public class FootballApiService : IFootballApiService
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
				try
				{
					Console.WriteLine($" ************************** Fetching fixtures for League {leagueId}");

					var url = $"https://v3.football.api-sports.io/fixtures?league={leagueId}&season={Season}";
					var request = new HttpRequestMessage(HttpMethod.Get, url);
					request.Headers.Add("x-apisports-key", _apiKey);
					var response = await _httpClient.SendAsync(request);
					response.EnsureSuccessStatusCode();

					var json = await response.Content.ReadAsStringAsync();

					// for debugging purposes
					if (string.IsNullOrWhiteSpace(json) || !json.Contains("response"))
					{
						Console.WriteLine($" ************************** Unexpected API response for league {leagueId}: {json}");
						continue;
					}

					var fixtures = JsonSerializer.Deserialize<FixtureApiResponse>(json, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					// for debugging purposes
					if (fixtures?.Response == null || fixtures.Response.Count == 0)
					{
						Console.WriteLine($" ************************** No fixtures found for league {leagueId}");
						continue;
					}

					foreach (var match in fixtures.Response)
					{
						if (await _dbContext.MatchResults.AnyAsync(m => m.ApiFixtureId == match.Fixture.Id))
							continue;

						var homeTeam = await _dbContext.Teams
										   .FirstOrDefaultAsync(t => t.Name == match.Teams.Home.Name)
									   ?? await _dbContext.Teams.FirstOrDefaultAsync(t => t.TeamId == ExtractTeamIdFromLogoUrl(match.Teams.Home.Logo));

						var awayTeam = await _dbContext.Teams
										   .FirstOrDefaultAsync(t => t.Name == match.Teams.Away.Name)
									   ?? await _dbContext.Teams.FirstOrDefaultAsync(t => t.TeamId == ExtractTeamIdFromLogoUrl(match.Teams.Away.Logo));

						if (homeTeam != null && awayTeam != null)
						{
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
						else
						{
							// for debugging purposes
							Console.WriteLine($"************************** Skipped fixture {match.Fixture.Id}: Home or away team not found in DB.");
						}
					}

					await _dbContext.SaveChangesAsync();
					await Task.Delay(6000); // API LIMITATIONS ----------> 1 request every 6 seconds. 10 requests per minute 
				}
				catch (Exception ex)
				{
					// for debugging purposes
					Console.WriteLine($"************************** Error fetching league {leagueId}: {ex.Message}");
				}
			}
		}



		public async Task FetchAndSaveTeamsAsync()
		{
			var leagues = new Dictionary<int, string>
			{
				{ PremierLeagueId, PremierLeagueName },
				{ LaLigaId, LaLigaName },
				{ BundesligaId, BundesligaName },
				{ SeriaAId, SeriaAName },
				{ Ligue1Id, Ligue1Name },
				{ EredivisieId, EredivisieName },
				{ LigaPortugalId, LigaPortugalName },
				{ PolishLeagueId, PolishLeagueName },
				{ SwissLeagueId, SwissLeagueName },
				{ BulgarianLeagueId, BulgarianLeagueName }
			};

			foreach (var kvp in leagues)
			{
				int leagueId = kvp.Key;
				string leagueName = kvp.Value;

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
								LogoUrl = teamInfo.Team.Logo,
								LeagueName = leagueName // Set from dictionary
							};

							await _dbContext.Teams.AddAsync(team);
						}
					}
				}
				await Task.Delay(6000); // API LIMITATIONS ----------> 1 request every 6 seconds. 10 requests per minute 
			}

			await _dbContext.SaveChangesAsync();
		}


		// Helper method to extract team ID
		public int? ExtractTeamIdFromLogoUrl(string logoUrl)
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

		public async Task<List<UpcomingMatchDto>> GetUpcomingFromApiAsync(
			int leagueId,
			int? limit = null)
		{
			// Season START year (API-Football rule)
			var season = DateTime.UtcNow.Month >= 7
				? DateTime.UtcNow.Year
				: DateTime.UtcNow.Year - 1;

			var baseUrl =
				$"https://v3.football.api-sports.io/fixtures" +
				$"?league={leagueId}&season={season}";

			string url;

			// 🔥 Bulgaria: get ALL fixtures from today to season end
			if (SupportedApiLeagues.UnreliableNsStatusLeagues.Contains(leagueId))
			{
				var fromDate = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
				var toDate = new DateTime(season + 1, 6, 30).ToString("yyyy-MM-dd");

				url = $"{baseUrl}&from={fromDate}&to={toDate}";
			}
			else
			{
				// Normal leagues (NS works fine)
				url = limit.HasValue
					? $"{baseUrl}&next={limit.Value}"
					: $"{baseUrl}&status=NS";
			}

			using var request = new HttpRequestMessage(HttpMethod.Get, url);
			request.Headers.Add("x-apisports-key", _apiKey);

			var response = await _httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync();

			var data = JsonSerializer.Deserialize<FixtureApiResponse>(
				json,
				new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
			);

			if (data?.Response == null)
				return new();

			return data.Response
				// ✅ Only require a real DATE (time may be 00:00)
				.Where(f => f.Fixture.Date.Year > 2000)
				.OrderBy(f => f.Fixture.Date)
				.Select(f => new UpcomingMatchDto
				{
					ApiFixtureId = f.Fixture.Id,
					KickoffUtc = f.Fixture.Date,

					HomeName = f.Teams.Home.Name,
					AwayName = f.Teams.Away.Name,

					HomeLogo = f.Teams.Home.Logo,
					AwayLogo = f.Teams.Away.Logo,

					HomeOdds = 1.11m,
					DrawOdds = 2.22m,
					AwayOdds = 3.33m
				})
				.ToList();
		}





	}
}
