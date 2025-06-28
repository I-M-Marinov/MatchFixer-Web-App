using MatchFixer.Core.ViewModels.LogoQuiz;

namespace MatchFixer.Core.Contracts
{
	public interface ILogoQuizService
	{
		Task<LogoQuizQuestionViewModel> GenerateQuestionAsync(int currentScore);
		LogoQuizQuestionViewModel BuildAnsweredModel(string selectedAnswer, string correctAnswer, string logoUrl, List<string> originalOptions);
		Task<(string Message, int NewScore)> UpdateLogoQuizScoreAsync(Guid userId, bool isCorrect);
	}
}
