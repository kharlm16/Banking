using System;
using System.ComponentModel.DataAnnotations;
using KharlBanking.Interfaces;

namespace KharlBanking.Models.ViewModels
{
    public class AccountDetailsListViewModel
    {
		[Display(Name = "Account Number")]
		public string AccountNumber { get; set; }
		[Display(Name = "Account Name")]
		public string AccountName { get; set; }
		[Display(Name = "Balance")]
		public decimal Balance { get; set; }
		[Display(Name = "Account Opened")]
		public DateTime CreatedDate { get; set; }
	}

	public class WithdrawDepositViewModel : IBanking
	{
		[Display(Name = "Account Number")]
		public string AccountNumber { get; set; }
		[Display(Name = "Account Name")]
		public string AccountName { get; set; }
		[Display(Name = "Balance")]
		public decimal Balance { get; set; }
		[Required]
		[Range(1, Int32.MaxValue, ErrorMessage = "Please enter an amount.")]
		public decimal Amount { get; set; }
		public byte[] RowVersion { get; set; }
	}

	public class TransferViewModel : IBanking
	{
		[Display(Name = "Account Number")]
		public string AccountNumber { get; set; }
		[Display(Name = "Account Name")]
		public string AccountName { get; set; }
		[Display(Name = "Balance")]
		public decimal Balance { get; set; }
		[Required]
		[Range(1, Int32.MaxValue, ErrorMessage = "Please enter an amount.")]
		public decimal Amount { get; set; }
		[Required(ErrorMessage = "Please enter the Account Number you want to deposit to.")]
		[Display(Name = "Transfer To")]
		public string TransferToAccount { get; set; }
		public byte[] RowVersion { get; set; }
		public byte[] RecepientRowVersion { get; set; }
	}
}
