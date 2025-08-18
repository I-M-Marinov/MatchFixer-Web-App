﻿using MatchFixer.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

using static MatchFixer.Common.GeneralConstants.WalletServiceConstants;

namespace MatchFixer_Web_App.Controllers
{

	
	public class WalletController : Controller
	{
		private readonly IWalletService _walletService;
		private readonly ISessionService _sessionService;

		public WalletController(IWalletService walletService, ISessionService sessionService)
		{
			_walletService = walletService;
			_sessionService = sessionService;
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateWallet()
		{
			var wallet = await _walletService.CreateWalletAsync();

			TempData["SuccessMessage"] = WalletCreatedSuccessfully;
			return RedirectToAction("WalletDetails");
		}

		[HttpGet]
		public async Task<IActionResult> WalletDetails()
		{
			var timeZoneId = _sessionService.GetUserTimezone();

			var model = await _walletService.GetWalletViewModelAsync(timeZoneId);

			if (model == null)
				return View("NoWallet");

			return View("Wallet", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Deposit(decimal amount)
		{
			try
			{
				await _walletService.DepositAsync(amount, "Manual deposit");
				TempData["SuccessMessage"] = SuccessfullyDeposited(amount);
			}
			catch (ArgumentException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (InvalidOperationException ex)
			{
				TempData["ErrorMessage"] = WalletNotFound;
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
					TempData["SuccessMessage"] = SuccessfullyWithdrew(amount);
				}
				else
				{
					TempData["ErrorMessage"] = InsufficientBalanceForWithdrawal;
				}
			}
			catch (ArgumentException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (InvalidOperationException)
			{
				TempData["ErrorMessage"] = WalletNotFound;
			}

			return RedirectToAction("WalletDetails");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ClearHistory()
		{
			var result = await _walletService.ClearTransactionHistoryAsync();

			if (!result.Success)
			{
				TempData["ErrorMessage"] = result.Message;
			}
			else
			{
				TempData["SuccessMessage"] = result.Message;
			}

			return RedirectToAction("WalletDetails");
		}


	}
}
