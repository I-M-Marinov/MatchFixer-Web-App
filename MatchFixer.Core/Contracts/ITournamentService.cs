using MatchFixer.Core.ViewModels.Tournaments;

namespace MatchFixer.Core.Contracts
{
	public interface ITournamentService
	{
		Task<TournamentListViewModel> GetListAsync(Guid currentUserId, bool isAdmin);

		Task<TournamentDetailsViewModel?> GetDetailsAsync(Guid tournamentId, Guid currentUserId, bool isAdmin);

		Task<(bool Success, string Message, Guid? TournamentId)> CreateAsync(Guid currentUserId, CreateTournamentInputModel input, bool isAdmin);

		// Admin: create "The Pot" - a free-to-enter, admin-funded special tournament.
		Task<(bool Success, string Message, Guid? TournamentId)> CreatePotAsync(Guid currentUserId, CreatePotInputModel input);

		Task<(bool Success, string Message)> JoinAsync(Guid tournamentId, Guid currentUserId);

		Task<(bool Success, string Message)> PlaceBetAsync(Guid tournamentId, Guid currentUserId, IReadOnlyList<(Guid MatchId, string Pick)> legs, decimal stake);

		Task<(bool Success, string Message)> PostMessageAsync(Guid tournamentId, Guid currentUserId, string text);

		// Admin
		Task<(bool Success, string Message)> CancelAsync(Guid tournamentId);

		// Admin / background lifecycle - ends and pays out (or cancels + refunds) a tournament.
		Task<(bool Success, string Message)> SettleAsync(Guid tournamentId);

		// Background tick: flips Upcoming -> Running at start, settles Running past its end.
		Task RunLifecycleAsync(CancellationToken ct);
	}
}
