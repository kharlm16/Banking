using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KharlBanking.Data;
using KharlBanking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KharlBanking.Controllers
{
	[Authorize]
	public class BankingController : Controller
    {
		ApplicationDbContext _db;

		public BankingController(ApplicationDbContext db)
		{
			_db = db;
		}
		
        public IActionResult AccountDetails()
		{
			var user = Helpers.GetCurrentUser(_db, User);

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
		public IActionResult Deposit()
		{
			var user = Helpers.GetCurrentUser(_db, User);

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
		public IActionResult Deposit(WithdrawDepositViewModel model)
		{
			var user = Helpers.GetCurrentUser(_db, User);

			try
			{
				var transaction = Helpers.SaveFunds(TransactionType.Deposit, model, user, _db);
				return View("TransactionSummary", transaction);
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", new { message = ex.Message });
			}
		}

		[HttpGet]
		public IActionResult Withdraw()
		{
			var user = Helpers.GetCurrentUser(_db, User);

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
		public IActionResult Withdraw(WithdrawDepositViewModel model)
		{
			var user = Helpers.GetCurrentUser(_db, User);

			try
			{
				var transaction = Helpers.SaveFunds(TransactionType.Withdraw, model, user, _db);
				return View("TransactionSummary", transaction);
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", new { message = ex.Message });
			}
		}

		[HttpGet]
		public IActionResult Transfer()
		{
			var user = Helpers.GetCurrentUser(_db, User);

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
		public IActionResult Transfer(TransferViewModel model)
		{
			var user = Helpers.GetCurrentUser(_db, User);

			try
			{
				var transaction = Helpers.TransferFunds(model, user, _db);
				return View("TransactionSummary", transaction);
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", new { message = ex.Message });
			}
		}

		public IActionResult TransactionHistory()
		{
			var userId = User.Claims.Where(p => p.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").FirstOrDefault().Value;

			var history = _db.Transactions
				.AsNoTracking()
				.Where(p => p.UserId == userId)
				.OrderByDescending(p => p.TransactionDate)
				.ToList();

			return View(history);
		}

		public IActionResult Error(string message)
		{
			ViewBag.ErrorMessage = message;

			return View();
		}
    }
}