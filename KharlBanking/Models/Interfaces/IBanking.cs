using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KharlBanking.Models
{
    public interface IBanking
    {
		string AccountNumber { get; set; }
		string AccountName { get; set; }
		decimal Balance { get; set; }
		decimal Amount { get; set; }
		byte[] RowVersion { get; set; }
	}
}
