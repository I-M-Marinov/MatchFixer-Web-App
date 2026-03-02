namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Teams.DTO
{
	public class TeamNode
	{
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public string Logo { get; set; } = "";
		public string? LocalLogoUrl { get; init; } // Local stored logo
		public string EffectiveLogoUrl =>
			!string.IsNullOrWhiteSpace(LocalLogoUrl)
				? LocalLogoUrl
				: Logo;
	}
}
