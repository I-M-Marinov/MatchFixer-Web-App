using MatchFixer_Web_App.Areas.Admin.ViewModels.Email;

namespace MatchFixer_Web_App.Areas.Admin.Interfaces
{
	public interface IAdminEmailService
	{
		Task<int> CountRecipientsAsync(EmailBlastCommand cmd);
		Task<EmailBlastResult> SendAsync(EmailBlastCommand cmd);
	}
}
