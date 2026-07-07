namespace MatchFixer_Web_App.Areas.Admin.ViewModels.Email
{
	/// <summary>Represents a pre-built email template available in the Email Blast compose picker.</summary>
	public record EmailTemplateDto(string Id, string Label, string Subject, string Body);
}
