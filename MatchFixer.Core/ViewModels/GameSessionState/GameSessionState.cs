

namespace MatchFixer.Core.ViewModels.GameSessionState
{
	public class GameSessionState
	{
		public string UserId { get; set; } = null!;
		public int QuestionNumber { get; set; } = 1;
		public int Score { get; set; } = 0;
		public int TotalQuestions { get; set; } = 10;
		public bool LastQuestionAnswered { get; set; } = false;
	}
}
