namespace MatchFixer.Core.ViewModels.LogoQuiz
{
	public class LogoQuizQuestionViewModel
	{
		public string LogoUrl { get; set; } = null!;
		public string CorrectAnswer { get; set; } = null!;
		public List<string> OriginalOptions { get; set; } = new();
		public List<string> Options { get; set; } = new();
		public string? SelectedAnswer { get; set; } 
		public bool? IsCorrect { get; set; }
	}

}
