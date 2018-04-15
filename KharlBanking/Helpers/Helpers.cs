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
		
		public static bool HaveEnoughBalance(decimal balance, decimal Amount)
		{
			return balance >= Amount;
		}

		public static decimal AddBalance(bool deduct, decimal balance, decimal amount)
		{
			return balance + (deduct ? amount *= -1 : amount);
		}
	}
}
