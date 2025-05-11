
namespace MatchFixer.Core.ViewModels.MatchGuessGame
{
	public class MatchGuessGameViewModel
	{
		public int QuestionNumber { get; set; }
		public int TotalQuestions { get; set; }
		public int Score { get; set; }

		public int MatchId { get; set; }
		public string League { get; set; }

		public string HomeTeam { get; set; }
		public string AwayTeam { get; set; }

		public string HomeTeamLogo { get; set; }
		public string AwayTeamLogo { get; set; }
		public int? ActualHomeScore { get; set; }
		public int? ActualAwayScore { get; set; }
		public bool IsAnswered { get; set; }
		public int? UserHomeGuess { get; set; }
		public int? UserAwayGuess { get; set; }

		public bool? IsCorrect { get; set; }
	}

}
