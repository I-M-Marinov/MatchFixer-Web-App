using System.Text.Json;

using MatchFixer.Common.Enums;
using MatchFixer.Common.GeneralConstants;
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
		private readonly IChangeRequestNotifier _notifier;

		public ChangeRequestService(MatchFixerDbContext db, UserManager<ApplicationUser> userManager, IChangeRequestNotifier notifier)
		{
			_db = db;
			_userManager = userManager;
			_notifier = notifier;
		}

		private async Task NotifyCreatedAsync(ChangeRequest request, ApplicationUser user) =>
			await _notifier.RequestCreatedAsync(ToInfo(request, user), await GetPendingCountAsync());

		private async Task NotifyResolvedAsync(Guid requestId, string outcome) =>
			await _notifier.RequestResolvedAsync(requestId, outcome, await GetPendingCountAsync());

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

			var request = new ChangeRequest
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				Type = ChangeRequestType.NameChange,
				Payload = payload,
				Summary = $"{user.FirstName} {user.LastName}  ->  {firstName} {lastName}",
				Status = ChangeRequestStatus.Pending,
				RequestedAtUtc = DateTime.UtcNow
			};
			_db.ChangeRequests.Add(request);
			await _db.SaveChangesAsync();
			await NotifyCreatedAsync(request, user);

			return (true, "Your name change has been submitted and is awaiting admin approval.");
		}

		public async Task<(bool Success, string Message)> SubmitDateOfBirthChangeAsync(Guid userId, DateTime dateOfBirth)
		{
			var dob = dateOfBirth.Date;
			var now = DateTime.UtcNow;

			if (!BettingEligibilityService.HasRealDateOfBirth(dob) || dob > now.Date)
				return (false, AgeVerificationConstants.MsgInvalidDob);

			var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
			if (user == null)
				return (false, "User not found.");

			if (user.DateOfBirth.Date == dob)
				return (false, "That is already your date of birth.");

			var hasPending = await _db.ChangeRequests.AnyAsync(r =>
				r.UserId == userId &&
				r.Type == ChangeRequestType.DateOfBirthChange &&
				r.Status == ChangeRequestStatus.Pending);
			if (hasPending)
				return (false, "You already have a date of birth change awaiting approval.");

			var current = BettingEligibilityService.HasRealDateOfBirth(user.DateOfBirth)
				? user.DateOfBirth.ToString("dd MMM yyyy")
				: "not provided";

			var newAge = BettingEligibilityService.CalculateAge(dob, now);
			var isUnderage = newAge < AgeVerificationConstants.MinimumBettingAge;
			var ageNote = isUnderage
				? $" (age {newAge} - UNDER {AgeVerificationConstants.MinimumBettingAge}, betting paused)"
				: $" (age {newAge})";

			var payload = new DateOfBirthChangePayload
			{
				DateOfBirth = dob,
				BettingPausedOnSubmit = isUnderage,
				PreviouslyVerified = user.IsAgeVerified,
				PreviouslyVerifiedOnUtc = user.AgeVerifiedOnUtc
			};

			// Under 18: pause betting until an admin reviews it
			if (isUnderage)
			{
				user.IsAgeVerified = false;
				user.AgeVerifiedOnUtc = null;
				user.ModifiedOn = now;
			}

			var request = new ChangeRequest
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				Type = ChangeRequestType.DateOfBirthChange,
				Payload = JsonSerializer.Serialize(payload),
				Summary = $"{current}  ->  {dob:dd MMM yyyy}{ageNote}",
				Status = ChangeRequestStatus.Pending,
				RequestedAtUtc = now
			};
			_db.ChangeRequests.Add(request);
			await _db.SaveChangesAsync();
			await NotifyCreatedAsync(request, user);

			return (true, isUnderage
				? "Your date of birth change has been submitted for admin approval. Betting is paused until it is reviewed."
				: "Your date of birth change has been submitted and is awaiting admin approval.");
		}

		public async Task<ChangeRequestInfo?> GetPendingDateOfBirthChangeAsync(Guid userId)
		{
			var r = await _db.ChangeRequests
				.AsNoTracking()
				.Where(x => x.UserId == userId
						 && x.Type == ChangeRequestType.DateOfBirthChange
						 && x.Status == ChangeRequestStatus.Pending)
				.OrderByDescending(x => x.RequestedAtUtc)
				.FirstOrDefaultAsync();

			return r == null ? null : ToInfo(r, null);
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

			var approvedMessage = "Request approved and applied.";

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
			else if (r.Type == ChangeRequestType.DateOfBirthChange)
			{
				DateOfBirthChangePayload? payload;
				try { payload = JsonSerializer.Deserialize<DateOfBirthChangePayload>(r.Payload); }
				catch { payload = null; }

				if (payload == null || !BettingEligibilityService.HasRealDateOfBirth(payload.DateOfBirth))
					return (false, "The request data is invalid and cannot be applied.");

				var user = r.User ?? await _db.Users.FirstOrDefaultAsync(u => u.Id == r.UserId);
				if (user == null)
					return (false, "The requesting user no longer exists.");

				var now = DateTime.UtcNow;
				var dob = payload.DateOfBirth.Date;
				var isAdult = BettingEligibilityService.CalculateAge(dob, now) >= AgeVerificationConstants.MinimumBettingAge;

				// Approval also settles age verification
				user.DateOfBirth = dob;
				user.IsAgeVerified = isAdult;
				user.AgeVerifiedOnUtc = isAdult ? now : null;
				user.ModifiedOn = now;

				var res = await _userManager.UpdateAsync(user);
				if (!res.Succeeded)
					return (false, "Could not apply the change: " + string.Join(", ", res.Errors.Select(e => e.Description)));

				approvedMessage = isAdult
					? "Date of birth updated and the user's age is now verified."
					: $"Date of birth updated. The user is under {AgeVerificationConstants.MinimumBettingAge}, so betting stays disabled.";
			}

			r.Status = ChangeRequestStatus.Approved;
			r.ReviewedByUserId = adminId;
			r.ReviewedAtUtc = DateTime.UtcNow;
			r.ReviewNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
			await _db.SaveChangesAsync();
			await NotifyResolvedAsync(r.Id, "approved");

			return (true, approvedMessage);
		}

		public async Task<(bool Success, string Message)> RejectAsync(Guid requestId, Guid adminId, string? note)
		{
			var r = await _db.ChangeRequests.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == requestId);
			if (r == null)
				return (false, "Request not found.");
			if (r.Status != ChangeRequestStatus.Pending)
				return (false, "This request has already been handled.");

			// Restore verification paused by an under-18 request
			if (r.Type == ChangeRequestType.DateOfBirthChange && r.User != null)
			{
				DateOfBirthChangePayload? p;
				try { p = JsonSerializer.Deserialize<DateOfBirthChangePayload>(r.Payload); }
				catch { p = null; }

				if (p is { BettingPausedOnSubmit: true, PreviouslyVerified: true } && !r.User.IsAgeVerified)
				{
					r.User.IsAgeVerified = true;
					r.User.AgeVerifiedOnUtc = p.PreviouslyVerifiedOnUtc ?? DateTime.UtcNow;
					r.User.ModifiedOn = DateTime.UtcNow;
				}
			}

			r.Status = ChangeRequestStatus.Rejected;
			r.ReviewedByUserId = adminId;
			r.ReviewedAtUtc = DateTime.UtcNow;
			r.ReviewNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
			await _db.SaveChangesAsync();
			await NotifyResolvedAsync(r.Id, "rejected");

			return (true, "Request rejected.");
		}

		private static ChangeRequestInfo ToInfo(ChangeRequest r, ApplicationUser? user)
		{
			string? current = null, proposed = null;
			DateTime? proposedDate = null;

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
			else if (r.Type == ChangeRequestType.DateOfBirthChange)
			{
				try
				{
					var p = JsonSerializer.Deserialize<DateOfBirthChangePayload>(r.Payload);
					if (p != null)
					{
						proposedDate = p.DateOfBirth.Date;
						proposed = p.DateOfBirth.ToString("dd MMMM yyyy");
					}
				}
				catch { }

				if (user != null)
					current = BettingEligibilityService.HasRealDateOfBirth(user.DateOfBirth)
						? user.DateOfBirth.ToString("dd MMMM yyyy")
						: "Not provided";
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
				ProposedDate = proposedDate,
				RequestedAtUtc = r.RequestedAtUtc
			};
		}

		private static string TypeLabel(ChangeRequestType t) => t switch
		{
			ChangeRequestType.NameChange => "Name change",
			ChangeRequestType.DateOfBirthChange => "Date of birth change",
			_ => t.ToString()
		};
	}
}
