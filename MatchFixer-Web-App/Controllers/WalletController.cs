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
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateWallet()
		{
			var wallet = await _walletService.CreateWalletAsync();

			TempData["SuccessMessage"] = "Wallet created successfully.";
			return RedirectToAction("WalletDetails");
		}

		[HttpGet]
		public async Task<IActionResult> WalletDetails()
		{
			var wallet = await _walletService.GetWalletAsync();

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
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Deposit(decimal amount)
		{
			try
			{
				await _walletService.DepositAsync(amount, "Manual deposit");
				TempData["SuccessMessage"] = $"Successfully deposited {amount:0.00} EUR.";
			}
			catch (ArgumentException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (InvalidOperationException ex)
			{
				TempData["ErrorMessage"] = "Your wallet could not be found.";
			}

			return RedirectToAction("WalletDetails");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Withdraw(decimal amount)
		{
			try
			{
				var success = await _walletService.WithdrawAsync(amount, "Manual withdrawal");

				if (success)
				{
					TempData["SuccessMessage"] = $"Successfully withdrew {amount:0.00} EUR.";
				}
				else
				{
					TempData["ErrorMessage"] = "Insufficient balance for this withdrawal.";
				}
			}
			catch (ArgumentException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (InvalidOperationException)
			{
				TempData["ErrorMessage"] = "Your wallet could not be found.";
			}

			return RedirectToAction("WalletDetails");
		}


	}
}
