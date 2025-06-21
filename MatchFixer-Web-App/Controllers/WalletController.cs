using MatchFixer.Common.Enums;
using MatchFixer.Core.Contracts;
using MatchFixer.Core.ViewModels.Wallet;
using MatchFixer.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MatchFixer_Web_App.Controllers
{

	
	public class WalletController : Controller
	{
		private readonly IWalletService _walletService;

		public WalletController(IWalletService walletService)
		{
			_walletService = walletService;
		}

		[HttpPost]
		public async Task<IActionResult> CreateWallet()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!Guid.TryParse(userId, out var userGuid))
				return Unauthorized();

			var wallet = await _walletService.CreateWalletAsync(userGuid);

			TempData["Message"] = "Wallet created successfully.";
			return RedirectToAction("WalletDetails");
		}

		public async Task<IActionResult> WalletDetails()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!Guid.TryParse(userId, out var userGuid))
				return Unauthorized();

			var wallet = await _walletService.GetWalletAsync(userGuid);

			if (wallet == null)
				return View("NoWallet");

			var model = new WalletViewModel
			{
				Balance = wallet.Balance,
				Currency = wallet.Currency,
				Transactions = wallet.Transactions
					.OrderByDescending(t => t.CreatedAt)
					.Select(t => new WalletTransactionViewModel
					{
						CreatedAt = t.CreatedAt,
						Amount = t.Amount,
						Description = t.Description,
						TransactionType = t.TransactionType
					}).ToList()
			};

			return View("Wallet", model);
		}

	}
}
