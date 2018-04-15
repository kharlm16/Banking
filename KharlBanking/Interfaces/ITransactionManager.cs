using System.Collections.Generic;
using System.Threading.Tasks;
using KharlBanking.Models.Entities;
using KharlBanking.Models.ViewModels;

namespace KharlBanking.Interfaces
{
	public interface ITransactionManager
	{
		Task<List<Transaction>> GetTransactions(string userid);
		Task<Transaction> SaveFunds(TransactionType transactionType, WithdrawDepositViewModel model, ApplicationUser user);
		Task<Transaction> TransferFunds(TransferViewModel model, ApplicationUser user);
	}
}