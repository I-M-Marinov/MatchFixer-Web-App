using MatchFixer.Core.DTOs.Limits;

namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Limits
{
	public class LimitsPageViewModel
	{
		public BettingLimitsConfigDto Config { get; set; } = new();

		/// <summary>League / competition names that can be added as a row.</summary>
		public List<string> AvailableLeagues { get; set; } = new();

		/// <summary>Admin display names for the "last changed by" notes.</summary>
		public Dictionary<Guid, string> AdminNames { get; set; } = new();

		public string NameOf(Guid? userId)
			=> userId.HasValue && AdminNames.TryGetValue(userId.Value, out var name) ? name : "an admin";
	}
}
