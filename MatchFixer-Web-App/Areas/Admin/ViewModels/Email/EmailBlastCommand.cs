namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Email
{
	public class EmailBlastCommand
	{
		public enum RecipientMode { All, Role, Status, Single }
		public RecipientMode Mode { get; set; }
		public Guid? UserId { get; set; }
		public string? Role { get; set; }
		public string? Status { get; set; } // "active", "unconfirmed", "locked", "deleted"
		public bool OnlyConfirmed { get; set; } = true;

		public string Subject { get; set; } = "";
		public string BodyHtml { get; set; } = ""; // Admin-provided body (HTML)
		public string LogoUrl { get; set; } = "https://res.cloudinary.com/doorb7d6i/image/upload/v1747399160/matchFixer-logo_yfzk57.png"; // default
	}

	public record EmailBlastResult(int TotalRecipients, int Sent, int Skipped);
}
