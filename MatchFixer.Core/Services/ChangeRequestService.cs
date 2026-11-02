using System.Text.Json;

using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.ChangeRequests;
using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer.Core.Services
{
	public class ChangeRequestService : IChangeRequestService
	{
		private readonly MatchFixerDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;

		public ChangeRequestService(MatchFixerDbContext db, UserManager<ApplicationUser> userManager)
		{
			_db = db;
			_userManager = userManager;
		}

		public async Task<(bool Success, string Message)> SubmitNameChangeAsync(Guid userId, string firstName, string lastName)
		{
			firstName = firstName?.Trim() ?? string.Empty;
			lastName = lastName?.Trim() ?? string.Empty;

			if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
				return (false, "First and last name are required.");

			var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
			if (user == null)
				return (false, "User not found.");

			if (string.Equals(user.FirstName, firstName, StringComparison.Ordinal) &&
				string.Equals(user.LastName, lastName, StringComparison.Ordinal))
				return (false, "That is already your name.");

			var hasPending = await _db.ChangeRequests.AnyAsync(r =>
				r.UserId == userId &&
				r.Type == ChangeRequestType.NameChange &&
				r.Status == ChangeRequestStatus.Pending);
			if (hasPending)
				return (false, "You already have a name change awaiting approval.");

			var payload = JsonSerializer.Serialize(new NameChangePayload { FirstName = firstName, LastName = lastName });

			_db.ChangeRequests.Add(new ChangeRequest
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				Type = ChangeRequestType.NameChange,
				Payload = payload,
				Summary = $"{user.FirstName} {user.LastName}  ->  {firstName} {lastName}",
				Status = ChangeRequestStatus.Pending,
				RequestedAtUtc = DateTime.UtcNow
			});
			await _db.SaveChangesAsync();

			return (true, "Your name change has been submitted and is awaiting admin approval.");
		}

		public async Task<ChangeRequestInfo?> GetPendingNameChangeAsync(Guid userId)
		{
			var r = await _db.ChangeRequests
				.AsNoTracking()
				.Where(x => x.UserId == userId
						 && x.Type == ChangeRequestType.NameChange
						 && x.Status == ChangeRequestStatus.Pending)
				.OrderByDescending(x => x.RequestedAtUtc)
				.FirstOrDefaultAsync();

			return r == null ? null : ToInfo(r, null);
		}

		public async Task<IReadOnlyList<ChangeRequestInfo>> GetPendingAsync()
		{
			var rows = await _db.ChangeRequests
				.AsNoTracking()
				.Include(x => x.User)
				.Where(x => x.Status == ChangeRequestStatus.Pending)
				.OrderBy(x => x.RequestedAtUtc)
				.ToListAsync();

			return rows.Select(r => ToInfo(r, r.User)).ToList();
		}

		public Task<int> GetPendingCountAsync() =>
			_db.ChangeRequests.CountAsync(x => x.Status == ChangeRequestStatus.Pending);

		public async Task<(bool Success, string Message)> ApproveAsync(Guid requestId, Guid adminId, string? note)
		{
			var r = await _db.ChangeRequests.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == requestId);
			if (r == null)
				return (false, "Request not found.");
			if (r.Status != ChangeRequestStatus.Pending)
				return (false, "This request has already been handled.");

			if (r.Type == ChangeRequestType.NameChange)
			{
				NameChangePayload? payload;
				try { payload = JsonSerializer.Deserialize<NameChangePayload>(r.Payload); }
				catch { payload = null; }

				if (payload == null || string.IsNullOrWhiteSpace(payload.FirstName) || string.IsNullOrWhiteSpace(payload.LastName))
					return (false, "The request data is invalid and cannot be applied.");

				var user = r.User ?? await _db.Users.FirstOrDefaultAsync(u => u.Id == r.UserId);
				if (user == null)
					return (false, "The requesting user no longer exists.");

				user.FirstName = payload.FirstName.Trim();
				user.LastName = payload.LastName.Trim();

				var res = await _userManager.UpdateAsync(user);
				if (!res.Succeeded)
					return (false, "Could not apply the change: " + string.Join(", ", res.Errors.Select(e => e.Description)));
			}

			r.Status = ChangeRequestStatus.Approved;
			r.ReviewedByUserId = adminId;
			r.ReviewedAtUtc = DateTime.UtcNow;
			r.ReviewNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
			await _db.SaveChangesAsync();

			return (true, "Request approved and applied.");
		}

		public async Task<(bool Success, string Message)> RejectAsync(Guid requestId, Guid adminId, string? note)
		{
			var r = await _db.ChangeRequests.FirstOrDefaultAsync(x => x.Id == requestId);
			if (r == null)
				return (false, "Request not found.");
			if (r.Status != ChangeRequestStatus.Pending)
				return (false, "This request has already been handled.");

			r.Status = ChangeRequestStatus.Rejected;
			r.ReviewedByUserId = adminId;
			r.ReviewedAtUtc = DateTime.UtcNow;
			r.ReviewNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
			await _db.SaveChangesAsync();

			return (true, "Request rejected.");
		}

		private static ChangeRequestInfo ToInfo(ChangeRequest r, ApplicationUser? user)
		{
			string? current = null, proposed = null;

			if (r.Type == ChangeRequestType.NameChange)
			{
				try
				{
					var p = JsonSerializer.Deserialize<NameChangePayload>(r.Payload);
					if (p != null) proposed = $"{p.FirstName} {p.LastName}".Trim();
				}
				catch { }

				if (user != null) current = $"{user.FirstName} {user.LastName}".Trim();
			}

			var name = user?.FullName ?? user?.UserName ?? "Player";

			return new ChangeRequestInfo
			{
				Id = r.Id,
				UserName = name,
				UserEmail = user?.Email ?? string.Empty,
				Type = r.Type,
				TypeLabel = TypeLabel(r.Type),
				Summary = r.Summary ?? string.Empty,
				CurrentValue = current,
				ProposedValue = proposed,
				RequestedAtUtc = r.RequestedAtUtc
			};
		}

		private static string TypeLabel(ChangeRequestType t) => t switch
		{
			ChangeRequestType.NameChange => "Name change",
			_ => t.ToString()
		};
	}
}
