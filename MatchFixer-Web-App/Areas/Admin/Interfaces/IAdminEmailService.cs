using MatchFixer_Web_App.Areas.Admin.ViewModels.Email;

namespace MatchFixer_Web_App.Areas.Admin.Interfaces
{
	public interface IAdminEmailService
	{
		Task<int> CountRecipientsAsync(EmailBlastCommand cmd);
		Task<EmailBlastResult> SendAsync(EmailBlastCommand cmd);

		/// <summary>
		/// Returns the pre-built email templates available in the compose picker.
		/// The boosted-matches template is only included when at least one active boost exists.
		/// </summary>
		Task<List<EmailTemplateDto>> GetEmailTemplatesAsync();
	}
}
