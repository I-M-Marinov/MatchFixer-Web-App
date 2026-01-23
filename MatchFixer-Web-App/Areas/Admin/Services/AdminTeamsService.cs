using Azure;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Teams;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Teams.DTO;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Text.Json;
using MatchFixer.Common.VirtualLeagues;
using static MatchFixer.Common.ServiceConstants.FootballApiConstants;

namespace MatchFixer_Web_App.Areas.Admin.Services
{
	public class AdminTeamsService : IAdminTeamsService
	{
		private readonly HttpClient _http;
		private readonly MatchFixerDbContext _db;
		private readonly string _apiKey;
		public static IReadOnlyDictionary<int, string> LeagueMap => _leagueMap;

		private static readonly IReadOnlyDictionary<int, string> _leagueMap =
			new ReadOnlyDictionary<int, string>(new Dictionary<int, string>
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
				{ BulgarianLeagueId, BulgarianLeagueName },

				// Competition-only virtual league 
				{ VirtualLeagues.RestOfWorldId, VirtualLeagues.RestOfWorldName }
			});

		public AdminTeamsService(HttpClient http, MatchFixerDbContext db, IConfiguration config)
		{
			_http = http;
			_db = db;
			_apiKey = config["FootballApi:Key"]; 
		}

		public async Task<IReadOnlyList<TeamSearchResult>> SearchTeamsAsync(
			string name, CancellationToken ct = default)
		{
			var list = await QueryTeamsAsync(("name", name), ct);
			if (list.Count == 0)
				list = await QueryTeamsAsync(("search", name), ct);

			// No league here — we assign league at Add time
			return list.Select(r => new TeamSearchResult
			{
				ApiTeamId = r.Team.Id,
				Name = r.Team.Name,
				LogoUrl = r.Team.Logo,
				LeagueId = 0,           
				LeagueName = string.Empty 
			}).ToList();
		}

		private async Task<List<SearchTeamResponseItem>> QueryTeamsAsync(
			(string key, string value) mainParam,
			CancellationToken ct)
		{
			var url = $"https://v3.football.api-sports.io/teams?{mainParam.key}={Uri.EscapeDataString(mainParam.value)}";
			using var req = new HttpRequestMessage(HttpMethod.Get, url);
			req.Headers.Add("x-apisports-key", _apiKey);

			using var resp = await _http.SendAsync(req, ct);
			if (!resp.IsSuccessStatusCode) return new List<SearchTeamResponseItem>();

			var json = await resp.Content.ReadAsStringAsync(ct);
			var parsed = JsonSerializer.Deserialize<SearchTeamListApiResponse>(
				json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

			return parsed?.Response ?? new List<SearchTeamResponseItem>();
		}

		public async Task<PaginatedTeamsList<TeamListRow>> GetTeamsPageAsync(
			int page,
			int pageSize,
			int[]? leagueIds = null,
			CancellationToken ct = default)
		{
			page = Math.Max(1, page);
			pageSize = Math.Clamp(pageSize, 5, 200);

			HashSet<string>? leagueNamesFilter = null;
			if (leagueIds is { Length: > 0 })
			{
				leagueNamesFilter = new HashSet<string>(
					leagueIds.Where(id => _leagueMap.ContainsKey(id))
							 .Select(id => _leagueMap[id]),
					StringComparer.OrdinalIgnoreCase);

				if (leagueNamesFilter.Count == 0)
				{
					return new PaginatedTeamsList<TeamListRow>
					{
						Items = new List<TeamListRow>(),
						Page = 1,
						PageSize = pageSize,
						TotalCount = 0
					};
				}
			}

			IQueryable<Team> q = _db.Teams.AsNoTracking();

			if (leagueNamesFilter is not null)
				q = q.Where(t => leagueNamesFilter.Contains(t.LeagueName));

			q = q.OrderBy(t => t.LeagueName).ThenBy(t => t.Name);

			var total = await q.CountAsync(ct);

			var items = await q
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(t => new TeamListRow
				{
					Id = t.Id,
					ApiTeamId = t.TeamId,
					Name = t.Name,
					LogoUrl = t.LogoUrl,
					LeagueName = t.LeagueName
				})
				.ToListAsync(ct);

			return new PaginatedTeamsList<TeamListRow>
			{
				Items = items,
				Page = page,
				PageSize = pageSize,
				TotalCount = total
			};
		}

		public async Task<bool> AddTeamFromSearchAsync(int apiTeamId, string name, string logoUrl, int leagueId, CancellationToken ct = default)
		{
			var exists = await _db.Teams.AnyAsync(t => t.TeamId == apiTeamId, ct);
			if (exists) return false;

			var leagueName = _leagueMap.TryGetValue(leagueId, out var ln) ? ln : $"League {leagueId}";

			var team = new Team
			{
				Id = Guid.NewGuid(),
				TeamId = apiTeamId,
				Name = name,
				LogoUrl = logoUrl,
				LeagueName = leagueName
			};

			await _db.Teams.AddAsync(team, ct);
			await _db.SaveChangesAsync(ct);
			return true;
		}

		public Task<Dictionary<int, string>> GetAllLeaguesAsync(CancellationToken ct = default)
		{
			var dict = _leagueMap.OrderBy(kv => kv.Value)
				.ToDictionary(kv => kv.Key, kv => kv.Value);
			return Task.FromResult(dict);
		}
	}

}
