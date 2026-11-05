namespace MatchFixer_Web_App.Areas.Admin.ViewModels
{
	public class AdminUserRow
	{
		public Guid Id { get; set; }
		public string Email { get; set; } = "";
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string? UserImage { get; set; }
		public bool EmailConfirmed { get; set; }
		public bool IsLockedOut { get; set; }
		public DateTimeOffset? LockoutEnd { get; set; }
		public string[] Roles { get; set; } = Array.Empty<string>();
		public decimal? WalletBalance { get; set; }
		public bool IsWalletLocked { get; set; } = false;
		public int BetsCount { get; set; }
		public bool IsActive { get; set; }
		public bool IsDeleted { get; set; }

		// Betting access
		public DateTime DateOfBirth { get; set; }
		public bool IsAgeVerified { get; set; }
		public DateTime? AgeVerifiedOnUtc { get; set; }
		public bool IsBettingBlocked { get; set; }

		public bool HasDateOfBirth => MatchFixer.Core.Services.BettingEligibilityService.HasRealDateOfBirth(DateOfBirth);

		public int? Age => HasDateOfBirth
			? MatchFixer.Core.Services.BettingEligibilityService.CalculateAge(DateOfBirth, DateTime.UtcNow)
			: null;

		public MatchFixer.Core.DTOs.Betting.BettingEligibility Betting =>
			MatchFixer.Core.Services.BettingEligibilityService.Evaluate(IsBettingBlocked, IsAgeVerified, DateOfBirth, DateTime.UtcNow);
	}
}
