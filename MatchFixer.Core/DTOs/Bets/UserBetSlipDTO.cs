﻿namespace MatchFixer.Core.DTOs.Bets
{
	public class UserBetSlipDTO
	{
		public Guid Id { get; set; }
		public DateTime BetTime { get; set; }
		public string DisplayTime { get; set; }
		public decimal Amount { get; set; }
		public decimal TotalOdds { get; set; }
		public string Status { get; set; }
		public decimal? WinAmount { get; set; }
		public Guid UserId { get; set; }
		public List<SingleBetDto> Bets { get; set; } = new();

	}
}
