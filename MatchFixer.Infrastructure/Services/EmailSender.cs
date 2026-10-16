using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;

public class EmailSender : IEmailSender
{
	private readonly IConfiguration _configuration;

	public EmailSender(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public async Task SendEmailAsync(string email, string subject, string htmlMessage)
	{
		var fromAddress = _configuration["GmailSmtp:User"]
			?? throw new InvalidOperationException("GmailSmtp:User is not configured.");
		var appPassword = _configuration["GmailSmtp:Pass"]
			?? throw new InvalidOperationException("GmailSmtp:Pass is not configured.");

		var message = new MimeMessage();
		message.From.Add(MailboxAddress.Parse(fromAddress));
		message.To.Add(MailboxAddress.Parse(email));
		message.Subject = subject;
		message.Body = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();

		using var client = new SmtpClient();

		// Fail fast instead of hanging for minutes if the network misbehaves.
		client.Timeout = 20000; // 20 seconds

		// Gmail submission over STARTTLS on 587. MailKit tries every resolved
		// address and falls back from IPv6 to IPv4 - the old System.Net.Mail
		// SmtpClient did not, which is why it died inside the container.
		await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
		await client.AuthenticateAsync(fromAddress, appPassword);
		await client.SendAsync(message);
		await client.DisconnectAsync(true);
	}
}
