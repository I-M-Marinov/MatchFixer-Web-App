
using MatchFixer.Core.DTOs.Teams;

namespace MatchFixer.Core.Contracts
{
	public interface ITeamLogoSyncService
	{
		
		Task<TeamLogoSyncResult> SyncAllTeamLogosAsync(
			bool forceRedownload = false,
			CancellationToken ct = default);

		Task<TeamLogoSyncResult> SyncTeamLogoAsync(
			Guid teamId,
			bool forceRedownload = false,
			CancellationToken ct = default);

		Task<string> SaveCustomLogoAsync(
			Guid teamId,
			Stream content,
			long length,
			CancellationToken ct = default);

		bool LocalLogoExists(string? relativeUrl);

		bool IsCustomLogo(string? relativeUrl);
	}
}
