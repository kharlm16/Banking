using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KharlBanking.Models.Entities
{
    public class Transaction
    {
		public Transaction()
		{
			this.Id = Guid.NewGuid();
			this.TransactionDate = DateTime.Now;
		}

		[Required]
		public Guid Id { get; set; }
		[Required]
		[Display(Name = "Date of Transaction")]
		public DateTime TransactionDate { get; set; }
		[Required]
		public string CreatedById { get; set; }
		[Required]
		[Display(Name = "Transaction Type")]
		public TransactionType TransactionType { get; set; }
		[Required]
		public string UserId { get; set; }
		[Required]
		public decimal Amount { get; set; }
		[Required]
		[Display(Name = "Balance before transaction")]
		public decimal BalanceBefore { get; set; }
		[Required]
		[Display(Name = "Balance after transaction")]
		public decimal BalanceAfter { get; set; }
		public string Remarks { get; set; }

		public ApplicationUser CreatedBy { get; set; }
		public ApplicationUser User { get; set; }
	}
}
