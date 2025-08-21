using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MatchFixer.Infrastructure.Entities
{
	public class OddsBoost
	{
		public Guid Id { get; set; }

		[Required]
		public Guid MatchEventId { get; set; }
		public MatchEvent MatchEvent { get; set; } = null!;

		[Required]
		public DateTime StartUtc { get; set; } // must be < EndUtc
		[Required]
		public DateTime EndUtc { get; set; }

		[Precision(5, 2)]
		[Range(0.01, 9.99, ErrorMessage = "BoostValue must be between 0.01 and 9.99.")]
		public decimal BoostValue { get; set; }       // e.g. +0.15 odds bump

		[Precision(18, 2)]
		[Range(0.01, 9999999999999999.99, ErrorMessage = "Max stake must be greater than 0.")]
		public decimal? MaxStakePerBet { get; set; }  // optional, but must be positive if set

		[Range(1, int.MaxValue, ErrorMessage = "Max uses must be at least 1.")]
		public int? MaxUsesPerUser { get; set; }      // optional, but must be ≥ 1 if set

		public bool IsActive { get; set; } = true;

		[Required]
		public Guid CreatedByUserId { get; set; }

		[StringLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
		public string? Note { get; set; }
	}
}
