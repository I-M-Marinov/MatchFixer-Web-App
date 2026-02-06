using MatchFixer.Infrastructure.Contracts;
using MatchFixer.Infrastructure.Models.TheSportsDBAPI;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace MatchFixer.Infrastructure.Services
{
	public class TheSportsDbApiService : ITheSportsDbApiService
	{
		private readonly HttpClient _http;

		private const string BaseUrl = "https://www.thesportsdb.com/api/v1/json";
		private const string BaseUrlV2 = "https://www.thesportsdb.com/api/v2/json/livescore/soccer";
		private readonly string _apiKey;


		public TheSportsDbApiService(HttpClient http, IConfiguration configuration)
		{
			_http = http;
			_apiKey = configuration["TheSportsDBApi:Key"]
			         ?? throw new InvalidOperationException(
				         "TheSportsDB API key is missing.");
		}
		

		public async Task<List<LeagueTableRowApiDto>> GetLeagueTableAsync(
			int leagueId,
			string season,
			CancellationToken ct = default)
		{
			var url =
				$"{BaseUrl}/{_apiKey}/lookuptable.php?l={leagueId}&s={season}";

			HttpResponseMessage response;

			try
			{
				response = await _http.GetAsync(url, ct);
			}
			catch
			{
				return new();
			}

			if (!response.IsSuccessStatusCode)
				return new();

			string content;

			try
			{
				content = await response.Content.ReadAsStringAsync(ct);
			}
			catch
			{
				return new();
			}

			if (string.IsNullOrWhiteSpace(content))
				return new();

			LeagueTableApiResponse? parsed;

			try
			{
				parsed = JsonSerializer.Deserialize<LeagueTableApiResponse>(
					content,
					new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});
			}
			catch (JsonException)
			{
				return new();
			}

			return parsed?.Table ?? new();
		}

		public async Task<List<LiveEventApiDto>> GetLiveSoccerEventsAsync(
			CancellationToken ct = default)
		{
			var request = new HttpRequestMessage(
				HttpMethod.Get,
				BaseUrlV2
			);

			request.Headers.Add("X-API-KEY", _apiKey);

			var response = await _http.SendAsync(request, ct);

			if (!response.IsSuccessStatusCode)
				return new();

			var content = await response.Content.ReadAsStringAsync(ct);
			if (string.IsNullOrWhiteSpace(content))
				return new();

			var parsed = JsonSerializer.Deserialize<LiveScoreApiResponse>(
				content,
				new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

			return parsed?.Livescore ?? new();
		}



	}
}