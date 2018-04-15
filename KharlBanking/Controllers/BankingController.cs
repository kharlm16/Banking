using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using KharlBanking.Data;
using KharlBanking.Interfaces;
using KharlBanking.Models;
using KharlBanking.Models.Entities;
using KharlBanking.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KharlBanking.Controllers
{
	[Authorize]
	public class BankingController : Controller
    {
	    private ITransactionManager _transactionManager;
	    private UserManager<ApplicationUser> _userManager;

		public BankingController(ITransactionManager transactionManager, UserManager<ApplicationUser> userManager)
		{
			_transactionManager = transactionManager;
			_userManager = userManager;
		}
		
        public async Task<IActionResult> AccountDetails()
        {
	        var user = await _userManager.GetUserAsync(User);

			AccountDetailsListViewModel account = new AccountDetailsListViewModel()
			{
				AccountName = user.AccountName,
				AccountNumber = user.AccountNumber,
				Balance = user.Balance,
				CreatedDate = user.CreatedDate
			};

            return View(account);
        }

		[HttpGet]
		public async Task<IActionResult> Deposit()
		{
			var user = await _userManager.GetUserAsync(User);

			WithdrawDepositViewModel account = new WithdrawDepositViewModel()
			{
				AccountName = user.AccountName,
				AccountNumber = user.AccountNumber,
				Balance = user.Balance,
				RowVersion = user.RowVersion
			};

			return View(account);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Deposit(WithdrawDepositViewModel model)
		{
			var user = await _userManager.GetUserAsync(User);
			
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				var transaction = await _transactionManager.SaveFunds(TransactionType.Deposit, model, user);
				return View("TransactionSummary", transaction);
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", new { message = ex.Message });
			}
		}

		[HttpGet]
		public async Task<IActionResult> Withdraw()
		{
			var user = await _userManager.GetUserAsync(User);

			WithdrawDepositViewModel account = new WithdrawDepositViewModel()
			{
				AccountName = user.AccountName,
				AccountNumber = user.AccountNumber,
				Balance = user.Balance,
				RowVersion = user.RowVersion
			};

			return View(account);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Withdraw(WithdrawDepositViewModel model)
		{
			var user = await _userManager.GetUserAsync(User);

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				var transaction = await _transactionManager.SaveFunds(TransactionType.Withdraw, model, user);
				return View("TransactionSummary", transaction);
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", new { message = ex.Message });
			}
		}

		[HttpGet]
		public async Task<IActionResult> Transfer()
		{
			var user = await _userManager.GetUserAsync(User);

			TransferViewModel account = new TransferViewModel()
			{
				AccountName = user.AccountName,
				AccountNumber = user.AccountNumber,
				Balance = user.Balance,
				RowVersion = user.RowVersion
			};

			return View(account);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Transfer(TransferViewModel model)
		{
			var user = await _userManager.GetUserAsync(User);

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				var transaction = await _transactionManager.TransferFunds(model, user);
				return View("TransactionSummary", transaction);
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", new { message = ex.Message });
			}
		}

		public async Task<IActionResult> TransactionHistory()
		{
			var user = await _userManager.GetUserAsync(User);
			string userid = user.Id;
			var history = await _transactionManager.GetTransactions(userid);

			return View(history);
		}

		public async Task<IActionResult> Error(string message)
		{
			ViewBag.ErrorMessage = message;

			return View();
		}
    }
}