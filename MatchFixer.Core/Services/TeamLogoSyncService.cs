using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Teams;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MatchFixer.Core.Services
{
	public class TeamLogoSyncService : ITeamLogoSyncService
	{
		private readonly MatchFixerDbContext _dbContext;
		private readonly IWebHostEnvironment _env;
		private readonly ILogger<TeamLogoSyncService> _logger;
		private readonly HttpClient _httpClient;

		public TeamLogoSyncService(
			MatchFixerDbContext dbContext,
			IWebHostEnvironment env,
			ILogger<TeamLogoSyncService> logger,
			IHttpClientFactory httpClientFactory)
		{
			_dbContext = dbContext;
			_env = env;
			_logger = logger;
			_httpClient = httpClientFactory.CreateClient();
		}

		public async Task<TeamLogoSyncResult> SyncAllTeamLogosAsync(
			bool forceRedownload = false,
			CancellationToken ct = default)
		{
			var result = new TeamLogoSyncResult();

			var teams = await _dbContext.Teams.ToListAsync(ct);
			result.TotalProcessed = teams.Count;

			foreach (var team in teams)
			{
				var singleResult = await SyncTeamInternalAsync(team, forceRedownload, ct);

				result.Downloaded += singleResult.Downloaded;
				result.Skipped += singleResult.Skipped;
				result.Failed += singleResult.Failed;
				result.Errors.AddRange(singleResult.Errors);
			}

			await _dbContext.SaveChangesAsync(ct);

			_logger.LogInformation(
				"""
                ===========================================
                Team Logo Sync Completed
                Total: {Total}
                Downloaded: {Downloaded}
                Skipped: {Skipped}
                Failed: {Failed}
                ===========================================
                """,
				result.TotalProcessed,
				result.Downloaded,
				result.Skipped,
				result.Failed);

			return result;
		}

		public async Task<TeamLogoSyncResult> SyncTeamLogoAsync(
			Guid teamId,
			bool forceRedownload = false,
			CancellationToken ct = default)
		{
			var team = await _dbContext.Teams
				.FirstOrDefaultAsync(t => t.Id == teamId, ct);

			if (team == null)
				throw new InvalidOperationException("Team not found.");

			var result = await SyncTeamInternalAsync(team, forceRedownload, ct);
			await _dbContext.SaveChangesAsync(ct);

			return result;
		}

		private async Task<TeamLogoSyncResult> SyncTeamInternalAsync(
			Team team,
			bool forceRedownload,
			CancellationToken ct)
		{
			var result = new TeamLogoSyncResult { TotalProcessed = 1 };

			if (team.TeamId == null)
			{
				result.Skipped++;
				result.Errors.Add($"Team {team.Name} has no TeamId.");
				return result;
			}

			if (!forceRedownload && !string.IsNullOrEmpty(team.LocalLogoUrl))
			{
				result.Skipped++;
				return result;
			}

			if (string.IsNullOrWhiteSpace(team.LogoUrl))
			{
				result.Failed++;
				result.Errors.Add($"Team {team.Name} has no LogoUrl.");
				return result;
			}

			try
			{
				var bytes = await DownloadImageAsync(team.LogoUrl, ct);

				if (bytes == null || bytes.Length == 0)
				{
					result.Failed++;
					result.Errors.Add($"Failed to download logo for {team.Name}");
					return result;
				}

				var relativePath = await SaveLogoToDiskAsync(team.TeamId.Value, bytes, ct);

				team.LocalLogoUrl = relativePath;
				team.LogoLastSyncedUtc = DateTime.UtcNow;

				result.Downloaded++;

				_logger.LogInformation(
					"Logo synced for team {TeamName} ({TeamId})",
					team.Name,
					team.TeamId);
			}
			catch (Exception ex)
			{
				result.Failed++;
				result.Errors.Add($"{team.Name}: {ex.Message}");

				_logger.LogError(ex,
					"Error syncing logo for team {TeamName}",
					team.Name);
			}

			return result;
		}

		private async Task<byte[]?> DownloadImageAsync(string url, CancellationToken ct)
		{
			var response = await _httpClient.GetAsync(url, ct);

			if (!response.IsSuccessStatusCode)
				return null;

			return await response.Content.ReadAsByteArrayAsync(ct);
		}

		private async Task<string> SaveLogoToDiskAsync(
			int teamId,
			byte[] bytes,
			CancellationToken ct)
		{
			var folder = Path.Combine(_env.WebRootPath, "images", "teams");

			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);

			var fileName = $"{teamId}.png";
			var fullPath = Path.Combine(folder, fileName);

			await File.WriteAllBytesAsync(fullPath, bytes, ct);

			return $"/images/teams/{fileName}";
		}
	}
}