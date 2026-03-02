
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
	}
}
