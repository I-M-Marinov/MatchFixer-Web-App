using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

public class EmailSender : IEmailSender
{
	private readonly IConfiguration _configuration;

	public EmailSender(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public Task SendEmailAsync(string email, string subject, string htmlMessage)
	{
		var smtpClient = new SmtpClient("smtp.gmail.com")
		{
			Port = 587,
			Credentials = new NetworkCredential(
				_configuration["GmailSmtp:User"],
				_configuration["GmailSmtp:Pass"]
			),
			EnableSsl = true
		};

		var mailMessage = new MailMessage
		{
			From = new MailAddress(_configuration["GmailSmtp:User"]),
			Subject = subject,
			Body = htmlMessage,
			IsBodyHtml = true
		};

		mailMessage.To.Add(email);

		return smtpClient.SendMailAsync(mailMessage);
	}
}