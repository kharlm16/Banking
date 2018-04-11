﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KharlBanking.Models
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
		public decimal Amount { get; set; }
		[Required]
		[Display(Name = "Transfer To")]
		public string TransferToAccount { get; set; }
		public byte[] RowVersion { get; set; }
		public byte[] RecepientRowVersion { get; set; }
	}
}
