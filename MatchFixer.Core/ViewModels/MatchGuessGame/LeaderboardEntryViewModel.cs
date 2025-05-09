using MatchFixer.Infrastructure.Entities;

namespace MatchFixer.Core.ViewModels.MatchGuessGame
{
	public class LeaderboardEntryViewModel
	{
		public string UserName { get; set; } = null!;
		public string Picture { get; set; } = null!;
		public int Score { get; set; }
	}
}
