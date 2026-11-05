using MatchFixer_Web_App.Areas.Admin.ViewModels.Teams.DTO;

namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Teams
{
	public class TeamEditorVm
	{
		public Guid TeamId { get; set; }

		// Team fields
		public string Name { get; set; } = "";
		public string LogoUrl { get; set; } = "";
		public string? LocalLogoUrl { get; init; }         // Local stored logo

		public bool LocalLogoFileExists { get; init; }
		public bool IsCustomLogo { get; init; }
		public DateTime? LogoLastSyncedUtc { get; init; }

		public string EffectiveLogoUrl =>
			!string.IsNullOrWhiteSpace(LocalLogoUrl) && LocalLogoFileExists
				? LocalLogoUrl
				: LogoUrl;

		public string? WikiSummary { get; set; }

		public string? WikiImageUrl { get; set; }

		public string? WikipediaUrl { get; set; }

		public DateTime? WikiLastUpdatedUtc { get; set; }
		public string LeagueName { get; set; } = "";
		public int? ApiTeamId { get; set; }

		public int LeagueId { get; set; }
		public IReadOnlyList<LeagueOptionVm> Leagues { get; set; } = [];

		// Aliases
		public List<TeamAliasVm> Aliases { get; set; } = new();
	}
}
