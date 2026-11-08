using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MatchFixer.Infrastructure.Entities
{
	/// <summary>
	/// Site-wide betting switches managed on the admin Limits page. A single row (Id = 1).
	/// </summary>
	public class BettingLimitSettings
	{
		public const int SingletonId = 1;

		[Key]
		public int Id { get; set; } = SingletonId;

		[Comment("Over/Under goals market offered on the site")]
		public bool OverUnderEnabled { get; set; } = true;

		[Comment("Both Teams To Score market offered on the site")]
		public bool BothTeamsToScoreEnabled { get; set; } = true;

		[Comment("Double Chance market offered on the site")]
		public bool DoubleChanceEnabled { get; set; } = true;

		public DateTime? UpdatedAtUtc { get; set; }
		public Guid? UpdatedByUserId { get; set; }
	}
}
