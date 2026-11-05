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
		private const string TeamsFolderUrl = "/images/teams";
		private const string CustomFolderName = "custom";
		private const long MaxLogoBytes = 2 * 1024 * 1024;

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
			_httpClient.Timeout = TimeSpan.FromSeconds(20);

			// Some CDNs reject requests without a User-Agent.
			if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
				_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MatchFixer-LogoSync/1.0");
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
				// Never overwrite admin-uploaded logos
				var singleResult = await SyncTeamInternalAsync(team, forceRedownload, preserveCustom: true, ct);

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
                Errors: {Errors}
                ===========================================
                """,
				result.TotalProcessed,
				result.Downloaded,
				result.Skipped,
				result.Failed,
				string.Join(" | ", result.Errors));

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

			var result = await SyncTeamInternalAsync(team, forceRedownload, preserveCustom: false, ct);
			await _dbContext.SaveChangesAsync(ct);

			return result;
		}

		public async Task<string> SaveCustomLogoAsync(
			Guid teamId,
			Stream content,
			long length,
			CancellationToken ct = default)
		{
			if (length <= 0)
				throw new InvalidOperationException("The uploaded file is empty.");

			if (length > MaxLogoBytes)
				throw new InvalidOperationException("Logo must be 2 MB or smaller.");

			var team = await _dbContext.Teams.FirstOrDefaultAsync(t => t.Id == teamId, ct)
				?? throw new InvalidOperationException("Team not found.");

			using var ms = new MemoryStream();
			await content.CopyToAsync(ms, ct);
			var bytes = ms.ToArray();

			var extension = DetectImageExtension(bytes)
				?? throw new InvalidOperationException("Unsupported image. Use PNG, JPG, GIF or WEBP.");

			var folder = Path.Combine(_env.WebRootPath, "images", "teams", CustomFolderName);
			Directory.CreateDirectory(folder);

			var fileName = $"{team.Id:N}-{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
			await File.WriteAllBytesAsync(Path.Combine(folder, fileName), bytes, ct);

			var previous = team.LocalLogoUrl;

			team.LocalLogoUrl = $"{TeamsFolderUrl}/{CustomFolderName}/{fileName}";
			team.LogoLastSyncedUtc = DateTime.UtcNow;
			await _dbContext.SaveChangesAsync(ct);

			TryDeleteOldCustomLogo(previous, team.LocalLogoUrl);

			_logger.LogInformation("Custom logo uploaded for team {TeamName} ({Id})", team.Name, team.Id);

			return team.LocalLogoUrl;
		}

		public bool LocalLogoExists(string? relativeUrl)
		{
			var path = ResolvePhysicalPath(relativeUrl);
			return path != null && File.Exists(path);
		}

		public bool IsCustomLogo(string? relativeUrl) =>
			!string.IsNullOrWhiteSpace(relativeUrl) &&
			relativeUrl.StartsWith($"{TeamsFolderUrl}/{CustomFolderName}/", StringComparison.OrdinalIgnoreCase);

		private async Task<TeamLogoSyncResult> SyncTeamInternalAsync(
			Team team,
			bool forceRedownload,
			bool preserveCustom,
			CancellationToken ct)
		{
			var result = new TeamLogoSyncResult { TotalProcessed = 1 };

			var hasLocalFile = LocalLogoExists(team.LocalLogoUrl);

			if (preserveCustom && hasLocalFile && IsCustomLogo(team.LocalLogoUrl))
			{
				result.Skipped++;
				return result;
			}

			if (!forceRedownload && hasLocalFile)
			{
				result.Skipped++;
				return result;
			}

			if (string.IsNullOrWhiteSpace(team.LogoUrl))
			{
				result.Failed++;
				result.Errors.Add($"{team.Name}: no source logo URL");
				return result;
			}

			if (!Uri.TryCreate(team.LogoUrl, UriKind.Absolute, out _))
			{
				result.Failed++;
				result.Errors.Add($"{team.Name}: invalid source logo URL");
				return result;
			}

			try
			{
				var (bytes, error) = await DownloadImageAsync(team.LogoUrl, ct);

				if (bytes == null)
				{
					result.Failed++;
					result.Errors.Add($"{team.Name}: {error}");
					_logger.LogWarning("Logo download failed for {TeamName} from {Url}: {Error}", team.Name, team.LogoUrl, error);
					return result;
				}

				var extension = DetectImageExtension(bytes);
				if (extension == null)
				{
					result.Failed++;
					result.Errors.Add($"{team.Name}: source URL did not return an image");
					return result;
				}

				var baseName = team.TeamId?.ToString() ?? team.Id.ToString("N");
				var relativePath = await SaveLogoToDiskAsync(baseName, extension, bytes, ct);

				var previous = team.LocalLogoUrl;
				team.LocalLogoUrl = relativePath;
				team.LogoLastSyncedUtc = DateTime.UtcNow;
				TryDeleteOldCustomLogo(previous, relativePath);

				result.Downloaded++;

				_logger.LogInformation(
					"Logo synced for team {TeamName} ({TeamId})",
					team.Name,
					team.TeamId);
			}
			catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
			{
				result.Failed++;
				result.Errors.Add($"{team.Name}: {ex.Message}");

				_logger.LogError(ex,
					"Error syncing logo for team {TeamName}",
					team.Name);
			}

			return result;
		}

		private async Task<(byte[]? Bytes, string? Error)> DownloadImageAsync(string url, CancellationToken ct)
		{
			using var response = await _httpClient.GetAsync(url, ct);

			if (!response.IsSuccessStatusCode)
				return (null, $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");

			var bytes = await response.Content.ReadAsByteArrayAsync(ct);

			if (bytes.Length == 0)
				return (null, "empty response");

			if (bytes.Length > MaxLogoBytes * 5)
				return (null, "image too large");

			return (bytes, null);
		}

		private async Task<string> SaveLogoToDiskAsync(
			string baseName,
			string extension,
			byte[] bytes,
			CancellationToken ct)
		{
			var folder = Path.Combine(_env.WebRootPath, "images", "teams");
			Directory.CreateDirectory(folder);

			var fileName = $"{baseName}{extension}";
			await File.WriteAllBytesAsync(Path.Combine(folder, fileName), bytes, ct);

			return $"{TeamsFolderUrl}/{fileName}";
		}

		private void TryDeleteOldCustomLogo(string? previous, string? current)
		{
			if (!IsCustomLogo(previous) ||
			    string.Equals(previous, current, StringComparison.OrdinalIgnoreCase))
				return;

			try
			{
				var path = ResolvePhysicalPath(previous);
				if (path != null && File.Exists(path))
					File.Delete(path);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Could not delete old custom logo {Path}", previous);
			}
		}

		private string? ResolvePhysicalPath(string? relativeUrl)
		{
			if (string.IsNullOrWhiteSpace(relativeUrl) || string.IsNullOrEmpty(_env.WebRootPath))
				return null;

			var clean = relativeUrl.Split('?', '#')[0];
			if (!clean.StartsWith('/'))
				return null;

			var root = Path.GetFullPath(_env.WebRootPath);
			var full = Path.GetFullPath(Path.Combine(root, clean.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

			return full.StartsWith(root, StringComparison.OrdinalIgnoreCase) ? full : null;
		}

		private static string? DetectImageExtension(byte[] b)
		{
			if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47)
				return ".png";
			if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
				return ".jpg";
			if (b.Length >= 6 && b[0] == 'G' && b[1] == 'I' && b[2] == 'F' && b[3] == '8')
				return ".gif";
			if (b.Length >= 12 && b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F'
			    && b[8] == 'W' && b[9] == 'E' && b[10] == 'B' && b[11] == 'P')
				return ".webp";
			return null;
		}
	}
}
