using System.Text;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.LogoQuiz;
using MatchFixer.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer.Core.Services
{
	public class LogoQuizService : ILogoQuizService
	{
		private readonly MatchFixerDbContext _dbContext;
		private readonly Random _random = new();

		public LogoQuizService(MatchFixerDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<LogoQuizQuestionViewModel> GenerateQuestionAsync(int currentScore)
		{

			var teams = await _dbContext.Teams.ToListAsync();

			// correct answer
			var correctTeam = teams[_random.Next(teams.Count)];
			var firstLetter = char.ToUpper(correctTeam.Name[0]);

			// teams by the same starting letter
			var sameLetterTeams = teams
				.Where(t => t.Id != correctTeam.Id && char.ToUpper(t.Name[0]) == firstLetter)
				.ToList();

			var wrongOptions = new List<string>();

			if (sameLetterTeams.Count >= 3)
			{
				// incorrect answers
				wrongOptions = sameLetterTeams
					.OrderBy(_ => _random.Next())
					.Take(3)
					.Select(t => t.Name)
					.ToList();
			}
			else
			{
				// incorrect answers
				wrongOptions = sameLetterTeams
					.Select(t => t.Name)
					.ToList();

				var needed = 3 - wrongOptions.Count;
				var additionalOptions = teams
					.Where(t => t.Id != correctTeam.Id && !wrongOptions.Contains(t.Name))
					.OrderBy(_ => _random.Next())
					.Take(needed)
					.Select(t => t.Name);

				wrongOptions.AddRange(additionalOptions);
			}

			var allOptions = wrongOptions
				.Append(correctTeam.Name)
				.OrderBy(_ => _random.Next()) // shuffle the answers, so the 1st one is not always the correct one
				.ToList();

			return new LogoQuizQuestionViewModel
			{
				LogoUrl = correctTeam.LogoUrl,
				CorrectAnswer = correctTeam.Name,
				Options = allOptions,
				OriginalOptions = allOptions,
				CurrentScore = currentScore
			};
		}

		public LogoQuizQuestionViewModel BuildAnsweredModel(
			string selectedAnswer,
			string correctAnswer,
			string logoUrl,
			List<string> originalOptions)
		{
			return new LogoQuizQuestionViewModel
			{
				CorrectAnswer = correctAnswer,
				SelectedAnswer = selectedAnswer,
				IsCorrect = selectedAnswer == correctAnswer,
				LogoUrl = logoUrl,
				Options = originalOptions,
				OriginalOptions = originalOptions
			};
		}

		public async Task<(string Message, int NewScore)> UpdateLogoQuizScoreAsync(Guid userId, bool isCorrect)
		{
			var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
			StringBuilder message = new StringBuilder();

			if (user == null)
			{
				return ("User not found", 0);
			}

			var currentScore = user.LogoQuizScore;

			if (isCorrect)
			{
				if (currentScore >= 750)
				{
					user.LogoQuizScore += 3;
					message.AppendLine("Correct! You have earned 3 points!");
				}
				else if (currentScore >= 500)
				{
					user.LogoQuizScore += 2;
					message.AppendLine("Correct! You have earned 2 points!");
				}
				else
				{
					user.LogoQuizScore += 1;
					message.AppendLine("Correct! You have earned 1 point!");
				}
			}
			else
			{
				if (currentScore > 1)
				{
					if (currentScore > 750)
					{
						user.LogoQuizScore = (int)Math.Floor(currentScore * 0.85);
						message.AppendLine("Incorrect! Your score was reduced by 15%!");
					}
					else if (currentScore >= 500)
					{
						user.LogoQuizScore = (int)Math.Floor(currentScore * 0.7);
						message.AppendLine("Incorrect! Your score was reduced by 30%!");
					}
					else
					{
						user.LogoQuizScore = (int)Math.Floor(currentScore * 0.5);
						message.AppendLine("Incorrect! Your score was reduced by 50%!");
					}
				}
			}

			await _dbContext.SaveChangesAsync();
			return (message.ToString(), user.LogoQuizScore);
		}



	}

}
