using MatchFixer.Core.ViewModels.LogoQuiz;

namespace MatchFixer.Core.Contracts
{
	public interface ILogoQuizService
	{
		Task<LogoQuizQuestionViewModel> GenerateQuestionAsync();

		LogoQuizQuestionViewModel BuildAnsweredModel(string selectedAnswer, string correctAnswer, string logoUrl, List<string> originalOptions);
	}
}
