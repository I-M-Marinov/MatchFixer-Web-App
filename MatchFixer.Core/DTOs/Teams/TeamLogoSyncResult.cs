namespace MatchFixer.Core.DTOs.Teams
{
	public class TeamLogoSyncResult
	{
		public int TotalProcessed { get; set; }
		public int Downloaded { get; set; }
		public int Skipped { get; set; }
		public int Failed { get; set; }

		public List<string> Errors { get; set; } = new();
	}
}
