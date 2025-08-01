﻿using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.LogoQuiz;
using MatchFixer.Infrastructure;
using Microsoft.EntityFrameworkCore;

using static MatchFixer.Common.GeneralConstants.LogoQuizConstants;

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
			if (user == null)
			{
				return (UserNotFound, 0);
			}

			var currentScore = user.LogoQuizScore;
			string message;

			if (isCorrect)
			{
				int earnedPoints = currentScore >= ThresholdHigh ? PointsHigh
					: currentScore >= ThresholdMid ? PointsMid
					: PointsLow;

				user.LogoQuizScore += earnedPoints;
				message = CorrectAnswer(earnedPoints);
			}
			else
			{
				if (currentScore <= 1)
				{
					message = NoPenalty;
				}
				else
				{
					double penaltyFactor = currentScore > ThresholdHigh ? PenaltyHigh
						: currentScore >= ThresholdMid ? PenaltyMid
						: PenaltyLow;

					int reducedScore = (int)Math.Floor(currentScore * penaltyFactor);
					user.LogoQuizScore = reducedScore;

					int penaltyPercent = 100 - (int)(penaltyFactor * 100);
					message = IncorrectAnswer(penaltyPercent);
				}
			}

			await _dbContext.SaveChangesAsync();
			return (message, user.LogoQuizScore);
		}




	}

}
