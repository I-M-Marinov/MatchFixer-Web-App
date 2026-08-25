using MatchFixer.Core.Contracts;
using MatchFixer.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

using static MatchFixer.Common.GeneralConstants.WalletServiceConstants;

using MatchFixer.Common.GeneralConstants;
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

			TempData[TempDataKeys.SuccessMessage] = WalletCreatedSuccessfully;
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
				await _walletService.DepositAsync(amount, UserManualDeposit);
				TempData[TempDataKeys.SuccessMessage] = SuccessfullyDeposited(amount);
			}
			catch (ArgumentException ex)
			{
				TempData[TempDataKeys.ErrorMessage] = ex.Message;
			}
			catch (InvalidOperationException ex)
			{
				TempData[TempDataKeys.ErrorMessage] = WalletNotFound;
			}
			catch (WalletLockedException ex)
			{
				TempData[TempDataKeys.ErrorMessage] = ex.Message;
			}

			return RedirectToAction("WalletDetails");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Withdraw(decimal amount)
		{
			try
			{
				var success = await _walletService.WithdrawAsync(amount, UserManualWithdrawal);

				if (success)
				{
					TempData[TempDataKeys.SuccessMessage] = SuccessfullyWithdrew(amount);
				}
				else
				{
					TempData[TempDataKeys.ErrorMessage] = InsufficientBalanceForWithdrawal;
				}
			}
			catch (ArgumentException ex)
			{
				TempData[TempDataKeys.ErrorMessage] = ex.Message;
			}
			catch (InvalidOperationException)
			{
				TempData[TempDataKeys.ErrorMessage] = WalletNotFound;
			}
			catch (WalletLockedException ex)
			{
				TempData[TempDataKeys.ErrorMessage] = ex.Message;
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
				TempData[TempDataKeys.ErrorMessage] = result.Message;
			}
			else
			{
				TempData[TempDataKeys.SuccessMessage] = result.Message;
			}

			return RedirectToAction("WalletDetails");
		}

	}
}
