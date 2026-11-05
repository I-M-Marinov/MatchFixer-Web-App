using MatchFixer.Common.Enums;

namespace MatchFixer.Core.ViewModels.ChangeRequests
{
	// Stored JSON shape for a NameChange request.
	public class NameChangePayload
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}

	// Stored JSON shape for a DateOfBirthChange request.
	public class DateOfBirthChangePayload
	{
		public DateTime DateOfBirth { get; set; }

		// Restored if an under-18 request is rejected
		public bool BettingPausedOnSubmit { get; set; }
		public bool PreviouslyVerified { get; set; }
		public DateTime? PreviouslyVerifiedOnUtc { get; set; }
	}

	// Display model for a change request (user-side status + admin list).
	public class ChangeRequestInfo
	{
		public Guid Id { get; set; }
		public string UserName { get; set; }
		public string UserEmail { get; set; }
		public ChangeRequestType Type { get; set; }
		public string TypeLabel { get; set; }
		public string Summary { get; set; }
		public string? CurrentValue { get; set; }
		public string? ProposedValue { get; set; }
		public DateTime? ProposedDate { get; set; }
		public DateTime RequestedAtUtc { get; set; }
	}
}
