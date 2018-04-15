using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KharlBanking.Data;
using KharlBanking.Interfaces;
using KharlBanking.Models.Entities;
using KharlBanking.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace KharlBanking.Models
{
    public class TransactionManager : ITransactionManager
    {
	    private ApplicationDbContext _db;
	    private ApplicationUser _user;

	    public TransactionManager(ApplicationDbContext db)
	    {
		    _db = db;
	    }

	    public async Task<List<Transaction>> GetTransactions(string userid)
	    {
		    var history = await _db.Transactions
			    .AsNoTracking()
			    .Where(p => p.UserId == userid)
			    .OrderByDescending(p => p.TransactionDate)
			    .ToListAsync();

		    return history;
	    }

	    public async Task<Transaction> SaveFunds(TransactionType transactionType, WithdrawDepositViewModel model, ApplicationUser user)
	    {
		    Transaction transaction = AdjustBalance(transactionType, model.Amount, model.RowVersion, user);

		    try
		    {
			    await _db.SaveChangesAsync();
			    return transaction;
		    }
		    catch (DbUpdateConcurrencyException ex)
		    {
			    throw new Exception("Another transaction is ongoing with your account. Please try again.", ex);
		    }
		}

	    public async Task<Transaction> TransferFunds(TransferViewModel model, ApplicationUser user)
	    {
		    Transaction transaction = AdjustBalance(TransactionType.Send, model.Amount, model.RowVersion, user, $"Tranferred to Account Number {model.TransferToAccount}.");

		    var recepient = await _db.Users.FirstOrDefaultAsync(p => p.AccountNumber == model.TransferToAccount);

		    if (recepient == null)
			    throw new Exception("Account does not exist");

		    AdjustBalance(TransactionType.Receive, model.Amount, recepient.RowVersion, recepient, $"Received from Account Number {user.AccountNumber}");

		    try
		    {
			    await _db.SaveChangesAsync();
			    return transaction;
		    }
		    catch (DbUpdateConcurrencyException)
		    {
			    throw new Exception("Another transaction is ongoing with your account. Please try again.");
		    }
	    }

		#region Helpers

		private Transaction AdjustBalance(TransactionType transactionType, decimal amount, byte[] rowVersion, ApplicationUser user, string remarks = null)
	    {
		    bool deduct = transactionType == TransactionType.Send || transactionType == TransactionType.Withdraw;
		    decimal balanceBefore = user.Balance;

		    if (deduct && !Helpers.HaveEnoughBalance(user.Balance, amount))
		    {
			    throw new Exception("Not enough funds to process your transaction.");
		    }

		    user.Balance = Helpers.AddBalance(deduct, user.Balance, amount);

		    _db.Entry(user).Property("RowVersion").OriginalValue = rowVersion;
		    _db.Entry(user).State = EntityState.Modified;

		    return AddTransaction(transactionType, amount, user, balanceBefore, remarks);
	    }

	    private Transaction AddTransaction(TransactionType transactionType, decimal amount, ApplicationUser user, decimal BalanceBefore, string remarks = null)
	    {
		    Transaction transaction = new Transaction()
		    {
			    CreatedById = user.Id,
			    BalanceBefore = BalanceBefore,
			    BalanceAfter = user.Balance,
			    TransactionType = transactionType,
			    UserId = user.Id,
			    Amount = amount,
			    Remarks = remarks
		    };

		    _db.Entry(transaction).State = EntityState.Added;

		    return transaction;
	    }

	    #endregion
	}
}
