using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MatchFixer.Infrastructure.Entities
{
	public class LiveMatchResult
	{
		[Key] [ForeignKey(nameof(MatchEvent))] 
		public Guid MatchEventId { get; set; } // Same as MatchEvent.Id

		public MatchEvent MatchEvent { get; set; }

		[Required] [Range(0, 20)] public int HomeScore { get; set; }

		[Required] [Range(0, 20)] public int AwayScore { get; set; }

		// True = Home won, False = Away won
		public bool? HomeWonOnPenalties { get; set; }

		[Required] public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

		public string? Notes { get; set; }
	}
}
