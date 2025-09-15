using MatchFixer.Infrastructure;
using MatchFixer.Infrastructure.Security;
using MatchFixer_Web_App.Areas.Admin.Interfaces;
using MatchFixer_Web_App.Areas.Admin.ViewModels.Wallet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchFixer_Web_App.Areas.Admin.Controllers
{
	[Area("Admin")]
	[AdminOnly]
	public class WalletController : Controller
	{
		private readonly IAdminWalletService _adminWalletService;
		private readonly MatchFixerDbContext _db;

		public WalletController(IAdminWalletService adminWalletService, MatchFixerDbContext db)
		{
			_adminWalletService = adminWalletService;
			_db = db;
		}

		[HttpGet]
		public async Task<IActionResult> ByUser(Guid id, string tz = "Europe/Sofia")
		{
			// Make sure the user exists
			var user = await _db.Users.AsNoTracking()
				.Where(u => u.Id == id)
				.Select(u => new { u.Id, u.UserName, u.Email })
				.FirstOrDefaultAsync();

			if (user == null) return NotFound();

			var vm = await _adminWalletService.GetWalletViewModelAsync(id, tz);
			if (vm == null)
			{
				vm = new AdminWalletViewModel
				{
					UserId = id,
					UserName = user.UserName,
					Email = user.Email,
					HasWallet = false,
					Balance = 0m,
					Currency = "EUR",
					Transactions = new()
				};
			}

			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Guid id)
		{
			var (ok, msg) = await _adminWalletService.CreateWalletForUserAsync(id);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = msg;
			return RedirectToAction(nameof(ByUser), new { id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Adjust(Guid id, decimal amount, string? note, bool withdraw = false)
		{
			var result = withdraw
				? await _adminWalletService.AdminWithdrawAsync(id, amount, note)
				: await _adminWalletService.AdminDepositAsync(id, amount, note);

			TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
			return RedirectToAction(nameof(ByUser), new { id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ClearHistory(Guid id)
		{
			var (ok, msg) = await _adminWalletService.ClearTransactionHistoryAsync(id);
			TempData[ok ? "SuccessMessage" : "ErrorMessage"] = msg;
			return RedirectToAction(nameof(ByUser), new { id });
		}
	}
}
