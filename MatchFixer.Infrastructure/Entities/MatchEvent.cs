﻿
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MatchFixer.Common.GeneralConstants.MatchEventValidation;

namespace MatchFixer.Infrastructure.Entities
{
	public class MatchEvent
	{
		[Key]
		public Guid Id { get; set; }

		public Guid HomeTeamId { get; set; }
		public Guid AwayTeamId { get; set; }

		[ForeignKey(nameof(HomeTeamId))]
		public Team HomeTeam { get; set; }

		[ForeignKey(nameof(AwayTeamId))]
		public Team AwayTeam { get; set; }

		[Required]
		public DateTime MatchDate { get; set; }

		[Range(OddsMinValue, OddsMaxValue)] 
		public decimal? HomeOdds { get; set; }

		[Range(OddsMinValue, OddsMaxValue)]
		public decimal? DrawOdds { get; set; }

		[Range(OddsMinValue, OddsMaxValue)]
		public decimal? AwayOdds { get; set; }

		public bool IsDerby { get; set; } = false; // default to false ( not a derby match ) 
		public LiveMatchResult? LiveResult { get; set; }
		public bool IsCancelled { get; set; } = false; 
		public ICollection<Bet> Bets { get; set; } = new List<Bet>();
	}
}
