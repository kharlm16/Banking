using KharlBanking;
using KharlBanking.Models;
using Xunit;

namespace KharlBanking.Tests
{
    public class HelpersTest
    {
		[Fact]
		public void StringifyTest()
		{
			var result = KharlBanking.Helpers.Stringify(234);

			Assert.Equal("00000234", result);
		}

		[Fact]
		public void HaveEnoughBalanceTest()
		{
			decimal balance = 100;
			decimal amount = 100;

			var result = KharlBanking.Helpers.HaveEnoughBalance(balance, amount);
			Assert.True(result);
		}

		[Fact]
		public void AddBalanceTest()
		{
			decimal balance = 100;
			decimal amount = 80;

			var result = KharlBanking.Helpers.AddBalance(true, balance, amount);

			Assert.Equal(20.00m, result);
		}

	}
}
