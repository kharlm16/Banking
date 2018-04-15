using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace KharlBanking.Models.Entities
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
		public ApplicationUser()
		{
			this.CreatedDate = DateTime.Now;
			this.Balance = 0.00m;
		}

		[Required]
		public string AccountNumber { get; set; }
		[Required]
		public string AccountName { get; set; }
		[Required]
		public decimal Balance { get; set; }
		[Required]
		public DateTime CreatedDate { get; set; }
		[Timestamp]
		public byte[] RowVersion { get; set; }

		public ICollection<Transaction> TransactionCreatedBys { get; set; }
		public ICollection<Transaction> Transactions { get; set; }
	}
}
