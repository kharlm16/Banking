using KharlBanking.Data;
using KharlBanking.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KharlBanking.Models.Entities;
using KharlBanking.Models.ViewModels;

namespace KharlBanking
{
	public static class Helpers
	{
		public static string GenerateAccountNumber(ApplicationDbContext _db)
		{
			var users = _db.Users;
			long MyNumber = 0;

			if (users.Count() > 0)
			{
				var last = users.Max(p => p.AccountNumber);
				MyNumber = Int64.Parse(last);
			}

			string AccountNumber = Stringify(++MyNumber);

			return AccountNumber;
		}

		public static string Stringify(long number)
		{
			string AccountNumber = number.ToString("D8");

			return AccountNumber;
		}

		//public static Transaction SaveFunds(TransactionType transactionType, WithdrawDepositViewModel model, ApplicationUser user, ApplicationDbContext _db = null)
		//{
		//	Transaction transaction = AdjustBalance(transactionType, model.Amount, model.RowVersion, user, ref _db);

		//	try
		//	{
		//		_db.SaveChanges();
		//		return transaction;
		//	}
		//	catch (DbUpdateConcurrencyException)
		//	{
		//		throw new Exception("Another transaction is ongoing with your account. Please try again.");
		//	}
		//}

		public static bool HaveEnoughBalance(decimal balance, decimal Amount)
		{
			return balance >= Amount;
		}

		public static decimal AddBalance(bool deduct, decimal balance, decimal amount)
		{
			return balance + (deduct ? amount *= -1 : amount);
		}

		//public static Transaction TransferFunds(TransferViewModel model, ApplicationUser user, ApplicationDbContext _db)
		//{
		//	Transaction transaction = AdjustBalance(TransactionType.Send, model.Amount, model.RowVersion, user, ref _db, $"Tranferred to Account Number {model.TransferToAccount}.");

		//	var recepient = _db.Users.Where(p => p.AccountNumber == model.TransferToAccount).FirstOrDefault();

		//	if (recepient == null)
		//		throw new Exception("Account does not exist");

		//	AdjustBalance(TransactionType.Receive, model.Amount, recepient.RowVersion, recepient, ref _db, $"Received from Account Number {user.AccountNumber}");

		//	try
		//	{
		//		_db.SaveChanges();
		//		return transaction;
		//	}
		//	catch (DbUpdateConcurrencyException)
		//	{
		//		throw new Exception("Another transaction is ongoing with your account. Please try again.");
		//	}
		//}

		//private static Transaction AdjustBalance(TransactionType transactionType, decimal amount, byte[] rowVersion, ApplicationUser user, ref ApplicationDbContext _db, string remarks = null)
		//{
		//	bool deduct = transactionType == TransactionType.Send || transactionType == TransactionType.Withdraw;
		//	decimal BalanceBefore = user.Balance;

		//	if (deduct && !HaveEnoughBalance(user.Balance, amount))
		//	{
		//		throw new Exception("Not enough funds to process your transaction.");
		//	}

		//	user.Balance = AddBalance(deduct, user.Balance, amount);

		//	_db.Entry(user).Property("RowVersion").OriginalValue = rowVersion;
		//	_db.Entry(user).State = EntityState.Modified;

		//	return AddTransaction(transactionType, amount, user, BalanceBefore, ref _db, remarks);
		//}

		//private static Transaction AddTransaction(TransactionType transactionType, decimal amount, ApplicationUser user, decimal BalanceBefore, ref ApplicationDbContext _db, string remarks = null)
		//{
		//	Transaction transaction = new Transaction()
		//	{
		//		CreatedById = user.Id,
		//		BalanceBefore = BalanceBefore,
		//		BalanceAfter = user.Balance,
		//		TransactionType = transactionType,
		//		UserId = user.Id,
		//		Amount = amount,
		//		Remarks = remarks
		//	};

		//	_db.Entry(transaction).State = EntityState.Added;

		//	return transaction;
		//}
	}
}
