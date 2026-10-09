using MatchFixer.Core.ViewModels.MatchResults;

namespace MatchFixer.Core.Contracts
{
	public interface IAutoScoringService
	{
		/// <summary>Pulls the live API state for every API-linked match awaiting a result.</summary>
		Task<AutoScorePreviewViewModel> GetAutoScoreCandidatesAsync();

		/// <summary>Scores the chosen finished matches (re-verifying against the API) and settles their bets.</summary>
		Task<AutoScoreConfirmResultViewModel> ConfirmAutoScoresAsync(IEnumerable<Guid> matchIds);
	}
}
