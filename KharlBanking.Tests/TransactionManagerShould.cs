using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using KharlBanking.Data;
using KharlBanking.Models;
using KharlBanking.Models.Entities;
using KharlBanking.Models.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KharlBanking.Tests
{
	public class TransactionManagerShould
	{
		private const string Connstring = "DataSource=:memory:";

		[Theory]
		[InlineData(TransactionType.Deposit)]
		[InlineData(TransactionType.Withdraw)]
		public async Task ThrowExceptionOnConcurrencyConflict(TransactionType tranType)
		{
			await BeginTestSql(async (service, db) =>
			{
				var userId = (await db.Users.FirstOrDefaultAsync()).Id;
				var chromeUser = await db.Users.FirstOrDefaultAsync(p => p.Id == userId);
				var edgeUser = await db.Users.FirstOrDefaultAsync(p => p.Id == userId);
				WithdrawDepositViewModel modelOnChrome = new WithdrawDepositViewModel { Amount = 100.00m, RowVersion = chromeUser.RowVersion };
				WithdrawDepositViewModel modelOnEdge = new WithdrawDepositViewModel { Amount = 250.00m, RowVersion = edgeUser.RowVersion };

				// The first user will try to withdraw or deposit
				await service.SaveFunds(TransactionType.Deposit, modelOnChrome, chromeUser);
				// The second user will also try but will throw a Concurrency exception.
				Exception result = await Assert.ThrowsAsync<Exception>(() => service.SaveFunds(TransactionType.Deposit, modelOnEdge, edgeUser));

				Assert.IsType<DbUpdateConcurrencyException>(result.InnerException);
			});
		}

		[Fact]
		public async Task ThrowExceptionOnConcurrencyConflictOnTransfer()
		{
			await BeginTestSql(async (service, db) =>
			{
				var senderId = (await db.Users.FirstOrDefaultAsync()).Id;
				var receiverId = (await db.Users.LastOrDefaultAsync()).Id;
				var chromeSender = await db.Users.FirstOrDefaultAsync(p => p.Id == senderId);
				var chromeReceiver = await db.Users.FirstOrDefaultAsync(p => p.Id == receiverId);
				var edgeSender = await db.Users.FirstOrDefaultAsync(p => p.Id == senderId);
				var edgeReceiver = await db.Users.FirstOrDefaultAsync(p => p.Id == receiverId);
				TransferViewModel modelOnChrome = new TransferViewModel()
				{
					TransferToAccount = chromeReceiver.AccountNumber,
					Amount = 50m,
					RowVersion = chromeSender.RowVersion,
					RecepientRowVersion = chromeReceiver.RowVersion
				};
				TransferViewModel modelOnEdge = new TransferViewModel()
				{
					TransferToAccount = edgeReceiver.AccountNumber,
					Amount = 250m,
					RowVersion = edgeSender.RowVersion,
					RecepientRowVersion = edgeReceiver.RowVersion
				};

				// The first user will try to transfer money
				await service.TransferFunds(modelOnChrome, chromeSender);
				// The second user will also try but will throw a Concurrency exception.
				Exception result = await Assert.ThrowsAsync<Exception>(() => service.TransferFunds(modelOnEdge, edgeSender));

				Assert.IsType<DbUpdateConcurrencyException>(result.InnerException);
			});
		}

		[Fact]
		public async Task AddBalanceOnDeposit()
		{
			await BeginTestSqlite(async (service, db) =>
			{
				var user = await db.Users.FirstOrDefaultAsync();
				decimal amountToDeposit = 100;
				decimal originalBalance = user.Balance;
				decimal expectedBalance = originalBalance + amountToDeposit;
				WithdrawDepositViewModel model = new WithdrawDepositViewModel { Amount = amountToDeposit, RowVersion = user.RowVersion };

				await service.SaveFunds(TransactionType.Deposit, model, user);
				var result = await db.Users.FirstOrDefaultAsync(p => p.Id == user.Id);

				Assert.Equal(expectedBalance, result.Balance);
			});
		}

		[Fact]
		public async Task HaveTransactionOnDeposit()
		{
			await BeginTestSqlite(async (service, db) =>
			{
				var user = await db.Users.FirstOrDefaultAsync();
				decimal amountToDeposit = 100;
				decimal originalBalance = user.Balance;
				decimal expectedBalance = originalBalance + amountToDeposit;
				WithdrawDepositViewModel model = new WithdrawDepositViewModel { Amount = amountToDeposit, RowVersion = user.RowVersion };

				await service.SaveFunds(TransactionType.Deposit, model, user);
				var result = await db.Transactions
					.Where(p => p.UserId == user.Id)
					.OrderByDescending(p => p.TransactionDate)
					.FirstOrDefaultAsync();

				Assert.Equal(originalBalance, result.BalanceBefore);
				Assert.Equal(expectedBalance, result.BalanceAfter);
				Assert.Equal(amountToDeposit, result.Amount);
				Assert.Equal(TransactionType.Deposit, result.TransactionType);
			});
		}

		[Fact]
		public async Task DeductBalanceOnWithdraw()
		{
			await BeginTestSqlite(async (service, db) =>
			{
				var user = await db.Users.FirstOrDefaultAsync();
				decimal amountToWithdraw = 250;
				decimal originalBalance = user.Balance;
				decimal expectedBalance = originalBalance - amountToWithdraw;
				WithdrawDepositViewModel model = new WithdrawDepositViewModel { Amount = amountToWithdraw, RowVersion = user.RowVersion };

				await service.SaveFunds(TransactionType.Withdraw, model, user);
				var result = await db.Users.FirstOrDefaultAsync(p => p.Id == user.Id);

				Assert.Equal(expectedBalance, result.Balance);
			});
		}

		[Fact]
		public async Task HaveTransactionOnWithdraw()
		{
			await BeginTestSqlite(async (service, db) =>
			{
				var user = await db.Users.FirstOrDefaultAsync();
				decimal amountToWithdraw = 100;
				decimal originalBalance = user.Balance;
				decimal expectedBalance = originalBalance - amountToWithdraw;
				WithdrawDepositViewModel model = new WithdrawDepositViewModel { Amount = amountToWithdraw, RowVersion = user.RowVersion };

				await service.SaveFunds(TransactionType.Withdraw, model, user);
				var result = await db.Transactions
					.Where(p => p.UserId == user.Id)
					.OrderByDescending(p => p.TransactionDate)
					.FirstOrDefaultAsync();

				Assert.Equal(originalBalance, result.BalanceBefore);
				Assert.Equal(expectedBalance, result.BalanceAfter);
				Assert.Equal(amountToWithdraw, result.Amount);
				Assert.Equal(TransactionType.Withdraw, result.TransactionType);
			});
		}

		[Fact]
		public async Task SendAndReceiveExpectedAmountOnTransfer()
		{
			await BeginTestSqlite(async (service, db) =>
			{
				var sender = await db.Users.FirstOrDefaultAsync();
				var receiver = await db.Users.LastOrDefaultAsync();
				decimal amountToTransfer = 450;
				decimal senderOrigBalance = sender.Balance;
				decimal receiverOrigBalance = receiver.Balance;
				decimal senderExpBalance = senderOrigBalance - amountToTransfer;
				decimal receiverExpBalance = receiverOrigBalance + amountToTransfer;
				TransferViewModel model = new TransferViewModel()
				{
					TransferToAccount = receiver.AccountNumber,
					Amount = amountToTransfer,
					RowVersion = sender.RowVersion,
					RecepientRowVersion = receiver.RowVersion
				};

				await service.TransferFunds(model, sender);
				var sendResult = await db.Users.FirstOrDefaultAsync(p => p.Id == sender.Id);
				var receiveResult = await db.Users.FirstOrDefaultAsync(p => p.Id == receiver.Id);

				Assert.Equal(senderExpBalance, sendResult.Balance);
				Assert.Equal(receiverExpBalance, receiveResult.Balance);
			});
		}

		[Fact]
		public async Task RecordSenderTransactionHistoryOnTransfer()
		{
			await BeginTestSqlite(async (service, db) =>
			{
				var sender = await db.Users.FirstOrDefaultAsync();
				var receiver = await db.Users.LastOrDefaultAsync();
				decimal amountToTransfer = 450;
				decimal senderOrigBalance = sender.Balance;
				decimal receiverOrigBalance = receiver.Balance;
				decimal senderExpBalance = senderOrigBalance - amountToTransfer;
				decimal receiverExpBalance = receiverOrigBalance + amountToTransfer;
				TransferViewModel model = new TransferViewModel()
				{
					TransferToAccount = receiver.AccountNumber,
					Amount = amountToTransfer,
					RowVersion = sender.RowVersion,
					RecepientRowVersion = receiver.RowVersion
				};

				await service.TransferFunds(model, sender);
				var sendResult = await db.Transactions.FirstOrDefaultAsync(p => p.UserId == sender.Id);
				var receiveResult = await db.Transactions.FirstOrDefaultAsync(p => p.UserId == receiver.Id);

				// Assert Sender
				Assert.Equal(senderOrigBalance, sendResult.BalanceBefore);
				Assert.Equal(senderExpBalance, sendResult.BalanceAfter);
				Assert.Equal(amountToTransfer, sendResult.Amount);
				Assert.Equal(TransactionType.Send, sendResult.TransactionType);
			});
		}

		[Fact]
		public async Task RecordReceiverTransactionHistoryOnTransfer()
		{
			await BeginTestSqlite(async (service, db) =>
			{
				var sender = await db.Users.FirstOrDefaultAsync();
				var receiver = await db.Users.LastOrDefaultAsync();
				decimal amountToTransfer = 450;
				decimal senderOrigBalance = sender.Balance;
				decimal receiverOrigBalance = receiver.Balance;
				decimal senderExpBalance = senderOrigBalance - amountToTransfer;
				decimal receiverExpBalance = receiverOrigBalance + amountToTransfer;
				TransferViewModel model = new TransferViewModel()
				{
					TransferToAccount = receiver.AccountNumber,
					Amount = amountToTransfer,
					RowVersion = sender.RowVersion,
					RecepientRowVersion = receiver.RowVersion
				};

				await service.TransferFunds(model, sender);
				var sendResult = await db.Transactions.FirstOrDefaultAsync(p => p.UserId == sender.Id);
				var receiveResult = await db.Transactions.FirstOrDefaultAsync(p => p.UserId == receiver.Id);

				Assert.Equal(receiverOrigBalance, receiveResult.BalanceBefore);
				Assert.Equal(receiverExpBalance, receiveResult.BalanceAfter);
				Assert.Equal(amountToTransfer, receiveResult.Amount);
				Assert.Equal(TransactionType.Receive, receiveResult.TransactionType);
			});
		}

		#region Helpers

		private async Task BeginTestSqlite(Func<TransactionManager, ApplicationDbContext, Task> testLogic)
		{
			using (SqliteConnection conn = new SqliteConnection(Connstring))
			{
				await conn.OpenAsync();

				var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseSqlite(conn)
				.Options;

				using (var db = new ApplicationDbContext(options))
				{
					await db.Database.EnsureCreatedAsync();

					await SeedDatabase(db);

					var service = new TransactionManager(db);

					// Test Logic...
					await testLogic(service, db);
				}
			}
		}

		private async Task BeginTestSql(Func<TransactionManager, ApplicationDbContext, Task> testLogic)
		{
			//using (SqliteConnection conn = new SqliteConnection(Connstring))
			//{
			// await conn.OpenAsync();

			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=aspnet-KharlBanking-89E43E83-2108-46EE-A354-E6FCC79E7F52;Trusted_Connection=True;MultipleActiveResultSets=true")
				.Options;

			using (var db = new ApplicationDbContext(options))
			{
				await db.Database.EnsureCreatedAsync();

				await SeedDatabase(db);

				var service = new TransactionManager(db);

				// Test Logic...
				await testLogic(service, db);
			}
			//}
		}

		private static async Task SeedDatabase(ApplicationDbContext db)
		{
			List<ApplicationUser> users = new List<ApplicationUser>
			{
				new ApplicationUser()
				{
					Id = Guid.NewGuid().ToString(),
					Email = "myemail@company.com",
					UserName = "myemail@company.com",
					AccountName = "Account Holder 1",
					AccountNumber = "00000888",
					Balance = 1_000.00m,
					CreatedDate = DateTime.Now,
					AccessFailedCount = 0,
					EmailConfirmed = false,
					LockoutEnabled = false,
					PhoneNumberConfirmed = false,
					TwoFactorEnabled = false,
				},
				new ApplicationUser()
				{
					Id = Guid.NewGuid().ToString(),
					Email = "myemail2@company.com",
					UserName = "myemail2@company.com",
					AccountName = "Account Holder 2",
					AccountNumber = "00000999",
					Balance = 1_000.00m,
					CreatedDate = DateTime.Now,
					AccessFailedCount = 0,
					EmailConfirmed = false,
					LockoutEnabled = false,
					PhoneNumberConfirmed = false,
					TwoFactorEnabled = false,
				}
			};

			foreach (var user in users)
			{
				var check = await db.Users.FirstOrDefaultAsync(p => p.Email == user.Email);
				if (check == null)
					await db.Users.AddAsync(user);
				else
				{
					check.Balance = 1_000.00m;
					db.Entry(check).State = EntityState.Modified;
				}
			}
			//await db.Users.AddRangeAsync(users);
			await db.SaveChangesAsync();
		}

		#endregion
	}
}
