using MatchFixer.Core.Contracts;
using MatchFixer.Core.DTOs.Betting;
using MatchFixer.Infrastructure;
using Microsoft.EntityFrameworkCore;

using static MatchFixer.Common.GeneralConstants.AgeVerificationConstants;

namespace MatchFixer.Core.Services
{
	public class BettingEligibilityService : IBettingEligibilityService
	{
		private readonly MatchFixerDbContext _db;
		private readonly IChangeRequestService _changeRequests;

		public BettingEligibilityService(MatchFixerDbContext db, IChangeRequestService changeRequests)
		{
			_db = db;
			_changeRequests = changeRequests;
		}

		public async Task<BettingEligibility> GetEligibilityAsync(Guid userId, CancellationToken ct = default)
		{
			if (userId == Guid.Empty)
				return new(false, BettingBlockReason.NotSignedIn, MsgNotSignedIn);

			var u = await _db.Users
				.AsNoTracking()
				.Where(x => x.Id == userId)
				.Select(x => new { x.IsBettingBlocked, x.IsAgeVerified, x.DateOfBirth })
				.FirstOrDefaultAsync(ct);

			if (u == null)
				return new(false, BettingBlockReason.NotSignedIn, MsgNotSignedIn);

			return Evaluate(u.IsBettingBlocked, u.IsAgeVerified, u.DateOfBirth, DateTime.UtcNow);
		}

		public static BettingEligibility Evaluate(bool isBettingBlocked, bool isAgeVerified, DateTime dateOfBirth, DateTime utcNow)
		{
			if (isBettingBlocked)
				return new(false, BettingBlockReason.BlockedByAdmin, MsgBlockedByAdmin);

			if (!isAgeVerified || !HasRealDateOfBirth(dateOfBirth))
				return new(false, BettingBlockReason.AgeNotVerified, MsgAgeNotVerified);

			if (CalculateAge(dateOfBirth, utcNow) < MinimumBettingAge)
				return new(false, BettingBlockReason.Underage, MsgUnderage);

			return BettingEligibility.Allowed;
		}

		public async Task<(bool Success, string Message)> SelfVerifyAgeAsync(Guid userId, DateTime dateOfBirth, CancellationToken ct = default)
		{
			var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
			if (user == null)
				return (false, MsgNotSignedIn);

			if (user.IsAgeVerified)
				return (true, MsgAlreadyVerified);

			var dob = dateOfBirth.Date;
			var now = DateTime.UtcNow;

			if (!HasRealDateOfBirth(dob) || dob > now.Date)
				return (false, MsgInvalidDob);

			// Changing an existing DOB goes through admin approval
			if (HasRealDateOfBirth(user.DateOfBirth) && user.DateOfBirth.Date != dob)
			{
				var (submitted, submitMsg) = await _changeRequests.SubmitDateOfBirthChangeAsync(userId, dob);
				return (false, submitted ? MsgDobChangeSubmitted : submitMsg);
			}

			user.DateOfBirth = dob;
			user.ModifiedOn = now;

			if (CalculateAge(dob, now) < MinimumBettingAge)
			{
				user.IsAgeVerified = false;
				user.AgeVerifiedOnUtc = null;
				await _db.SaveChangesAsync(ct);
				return (false, MsgUnderage);
			}

			user.IsAgeVerified = true;
			user.AgeVerifiedOnUtc = now;
			await _db.SaveChangesAsync(ct);

			return (true, MsgVerifiedSuccessfully);
		}

		public async Task<(bool Success, string Message)> AdminVerifyAgeAsync(Guid userId, DateTime? dateOfBirth, CancellationToken ct = default)
		{
			var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
			if (user == null)
				return (false, MsgUserNotFound);

			var now = DateTime.UtcNow;
			var dob = (dateOfBirth ?? user.DateOfBirth).Date;

			if (!HasRealDateOfBirth(dob) || dob > now.Date)
				return (false, MsgAdminNeedsDob);

			if (CalculateAge(dob, now) < MinimumBettingAge)
				return (false, $"Cannot verify: date of birth {dob:dd MMM yyyy} makes this user under {MinimumBettingAge}.");

			user.DateOfBirth = dob;
			user.IsAgeVerified = true;
			user.AgeVerifiedOnUtc = now;
			user.ModifiedOn = now;
			await _db.SaveChangesAsync(ct);

			return (true, $"Age verified for {user.FirstName} {user.LastName} (DOB {dob:dd MMM yyyy}).");
		}

		public async Task<(bool Success, string Message)> AdminResetAgeVerificationAsync(Guid userId, CancellationToken ct = default)
		{
			var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
			if (user == null)
				return (false, MsgUserNotFound);

			user.IsAgeVerified = false;
			user.AgeVerifiedOnUtc = null;
			user.ModifiedOn = DateTime.UtcNow;
			await _db.SaveChangesAsync(ct);

			return (true, $"Age verification reset for {user.FirstName} {user.LastName}. They must verify again before betting.");
		}

		public async Task<(bool Success, string Message)> AdminSetBettingBlockedAsync(Guid userId, bool blocked, CancellationToken ct = default)
		{
			var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
			if (user == null)
				return (false, MsgUserNotFound);

			user.IsBettingBlocked = blocked;
			user.ModifiedOn = DateTime.UtcNow;
			await _db.SaveChangesAsync(ct);

			return (true, blocked
				? $"Betting blocked for {user.FirstName} {user.LastName}."
				: $"Betting re-enabled for {user.FirstName} {user.LastName}.");
		}

		// 0001-01-01 and 1900-01-01 are placeholders, not real dates
		public static bool HasRealDateOfBirth(DateTime dob) => dob.Year > 1900;

		public static int CalculateAge(DateTime dob, DateTime utcNow)
		{
			var today = utcNow.Date;
			var age = today.Year - dob.Year;
			if (dob.Date > today.AddYears(-age)) age--;
			return age;
		}
	}
}
