namespace MatchFixer.Common.GeneralConstants
{
	public static class LogoQuizConstants
	{
		// Score thresholds
		public const int ThresholdMid = 500;
		public const int ThresholdHigh = 750;

		// Points awarded
		public const int PointsLow = 1;
		public const int PointsMid = 2;
		public const int PointsHigh = 3;

		// Penalty multipliers
		public const double PenaltyLow = 0.5;
		public const double PenaltyMid = 0.7;
		public const double PenaltyHigh = 0.85;


		public const string UserNotFound = "User not found.";
		public const string NoPenalty = "Incorrect! You have just one point, so no penalty for now!";

		public static string CorrectAnswer(int points)
			=> $"Correct! You have earned {points} point{(points > 1 ? "s" : "")}!";

		public static string IncorrectAnswer(int penaltyPercent)
			=> $"Incorrect! Your score was reduced by {penaltyPercent}%!";
	}
}
