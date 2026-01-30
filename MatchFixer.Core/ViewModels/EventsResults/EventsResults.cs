namespace MatchFixer.Core.ViewModels.EventsResults
{
	public class EventsResults
	{
		public Guid MatchEventId { get; set; }
		public string HomeTeam { get; set; } = string.Empty;
		public string AwayTeam { get; set; } = string.Empty;
		public string? CompetitionName { get; set; }
		public string LeagueName { get; set; } = null!; 
		public int HomeScore { get; set; }
		public int AwayScore { get; set; }

		public string? HomeLogoUrl { get; set; }
		public string? AwayLogoUrl { get; set; }
	}

}


