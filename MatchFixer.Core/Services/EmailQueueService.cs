using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer.Core.Services
{
	public class EmailQueueService : IEmailQueueService
	{
		private readonly MatchFixerDbContext _db;
		private readonly IEmailSender _emailSender;

		public EmailQueueService(MatchFixerDbContext db, IEmailSender emailSender)
		{
			_db = db;
			_emailSender = emailSender;
		}

		public async Task EnqueueAsync(string recipient, string subject, string bodyHtml, EmailQueueCategory category,
			Guid? recipientUserId = null, string? relatedInfo = null)
		{
			if (string.IsNullOrWhiteSpace(recipient))
				return;

			var queued = new QueuedEmail
			{
				Id = Guid.NewGuid(),
				Recipient = recipient.Trim(),
				RecipientUserId = recipientUserId,
				Subject = subject ?? string.Empty,
				BodyHtml = bodyHtml ?? string.Empty,
				Category = category,
				Status = EmailQueueStatus.Pending,
				CreatedAtUtc = DateTime.UtcNow,
				RelatedInfo = relatedInfo
			};

			await _db.QueuedEmails.AddAsync(queued);
			await _db.SaveChangesAsync();
		}

		public Task<int> GetPendingCountAsync()
			=> _db.QueuedEmails.CountAsync(x => x.Status == EmailQueueStatus.Pending);

		public async Task<IReadOnlyList<QueuedEmail>> GetReviewQueueAsync()
			=> await _db.QueuedEmails
				.AsNoTracking()
				.Where(x => x.Status == EmailQueueStatus.Pending
							|| x.Status == EmailQueueStatus.Approved
							|| x.Status == EmailQueueStatus.Failed)
				.OrderBy(x => x.CreatedAtUtc)
				.ToListAsync();

		public Task<QueuedEmail?> GetAsync(Guid id)
			=> _db.QueuedEmails.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

		public async Task<bool> UpdateContentAsync(Guid id, string subject, string bodyHtml)
		{
			var email = await _db.QueuedEmails.FirstOrDefaultAsync(x => x.Id == id);
			if (email == null || email.Status == EmailQueueStatus.Sent)
				return false;

			email.Subject = subject ?? string.Empty;
			email.BodyHtml = bodyHtml ?? string.Empty;
			await _db.SaveChangesAsync();
			return true;
		}

		public async Task<int> DropAsync(IEnumerable<Guid> ids, Guid adminId)
		{
			var set = ids?.ToList() ?? new List<Guid>();
			if (set.Count == 0)
				return 0;

			var emails = await _db.QueuedEmails
				.Where(x => set.Contains(x.Id) && x.Status != EmailQueueStatus.Sent)
				.ToListAsync();

			foreach (var email in emails)
			{
				email.Status = EmailQueueStatus.Dropped;
				email.ReviewedByUserId = adminId;
				email.ReviewedAtUtc = DateTime.UtcNow;
			}

			await _db.SaveChangesAsync();
			return emails.Count;
		}

		public async Task<int> SendNowAsync(IEnumerable<Guid> ids, Guid adminId)
		{
			var set = ids?.ToList() ?? new List<Guid>();
			if (set.Count == 0)
				return 0;

			var emails = await _db.QueuedEmails
				.Where(x => set.Contains(x.Id)
							&& (x.Status == EmailQueueStatus.Pending
								|| x.Status == EmailQueueStatus.Approved
								|| x.Status == EmailQueueStatus.Failed))
				.ToListAsync();

			var sent = 0;
			foreach (var email in emails)
			{
				try
				{
					await _emailSender.SendEmailAsync(email.Recipient, email.Subject, email.BodyHtml);
					email.Status = EmailQueueStatus.Sent;
					email.SentAtUtc = DateTime.UtcNow;
					email.ReviewedByUserId = adminId;
					email.ReviewedAtUtc = DateTime.UtcNow;
					email.AttemptCount++;
					email.LastError = null;
					sent++;
				}
				catch (Exception ex)
				{
					email.Status = EmailQueueStatus.Failed;
					email.AttemptCount++;
					email.LastError = ex.Message.Length > 1000 ? ex.Message.Substring(0, 1000) : ex.Message;
				}
			}

			await _db.SaveChangesAsync();
			return sent;
		}

		public async Task<int> ScheduleAsync(IEnumerable<Guid> ids, int intervalSeconds, Guid adminId)
		{
			var set = ids?.ToList() ?? new List<Guid>();
			if (set.Count == 0)
				return 0;

			if (intervalSeconds < 0)
				intervalSeconds = 0;

			var emails = await _db.QueuedEmails
				.Where(x => set.Contains(x.Id)
							&& (x.Status == EmailQueueStatus.Pending || x.Status == EmailQueueStatus.Failed))
				.OrderBy(x => x.CreatedAtUtc)
				.ToListAsync();

			var now = DateTime.UtcNow;
			var i = 0;
			foreach (var email in emails)
			{
				email.Status = EmailQueueStatus.Approved;
				email.SendAfterUtc = now.AddSeconds((long)intervalSeconds * i);
				email.ReviewedByUserId = adminId;
				email.ReviewedAtUtc = now;
				email.LastError = null;
				i++;
			}

			await _db.SaveChangesAsync();
			return emails.Count;
		}
	}
}
