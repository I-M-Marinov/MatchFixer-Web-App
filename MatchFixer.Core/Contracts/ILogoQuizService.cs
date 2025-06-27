using MatchFixer.Core.ViewModels.LogoQuiz;

namespace MatchFixer.Core.Contracts
{
	public interface ILogoQuizService
	{
		Task<LogoQuizQuestionViewModel> GenerateQuestionAsync();
	}
}
