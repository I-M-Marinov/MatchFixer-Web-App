using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MatchFixer.Infrastructure.Entities
{
	/// <summary>
	/// Stake and payout limits for slips by league and number of legs (admin Limits page).
	/// The row named <see cref="DefaultLeagueName"/> applies to every league without its own row,
	/// and fills any blank cell of a league row. A blank Default cell means "no limit".
	/// </summary>
	public class LeagueBetLimit
	{
		public const string DefaultLeagueName = "__default__";

		[Key]
		public Guid Id { get; set; }

		[Required]
		[StringLength(100)]
		[Comment("League or competition name, or __default__")]
		public string LeagueName { get; set; } = null!;

		// Max stake for slips with only Match Result (1X2) picks, by number of legs
		public decimal? MaxStake1 { get; set; }
		public decimal? MaxStake2 { get; set; }
		public decimal? MaxStake3 { get; set; }
		public decimal? MaxStake4 { get; set; }
		public decimal? MaxStake5Plus { get; set; }

		// Max stake for slips that contain any extra-market pick (O/U, BTTS, Double Chance)
		public decimal? ExtraMaxStake1 { get; set; }
		public decimal? ExtraMaxStake2 { get; set; }
		public decimal? ExtraMaxStake3 { get; set; }
		public decimal? ExtraMaxStake4 { get; set; }
		public decimal? ExtraMaxStake5Plus { get; set; }

		[Comment("Maximum potential return (stake x odds) for a slip")]
		public decimal? MaxWin { get; set; }

		public DateTime? UpdatedAtUtc { get; set; }
		public Guid? UpdatedByUserId { get; set; }

		public bool IsDefault => LeagueName == DefaultLeagueName;

		/// <summary>Stake limit for a slip of <paramref name="legs"/> legs (5+ shares one column).</summary>
		public decimal? StakeFor(int legs, bool extraMarkets) => (legs, extraMarkets) switch
		{
			( <= 1, false) => MaxStake1,
			(2, false) => MaxStake2,
			(3, false) => MaxStake3,
			(4, false) => MaxStake4,
			(_, false) => MaxStake5Plus,
			( <= 1, true) => ExtraMaxStake1,
			(2, true) => ExtraMaxStake2,
			(3, true) => ExtraMaxStake3,
			(4, true) => ExtraMaxStake4,
			(_, true) => ExtraMaxStake5Plus
		};
	}
}
