using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using KharlBanking.Controllers;
using KharlBanking.Interfaces;
using KharlBanking.Models.Entities;
using KharlBanking.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using Xunit;
using Moq;

namespace KharlBanking.Tests
{
	public class BankingControllerShould
	{
		private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
		private readonly Mock<ITransactionManager> _mockTransactionManager;
		private readonly BankingController _controller;

		public BankingControllerShould()
		{
			var dummyUser = new ApplicationUser
			{
				AccountName = "Account Holder",
				AccountNumber = "00000001",
				CreatedDate = DateTime.Now,
				RowVersion = new byte[] {0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20}
			};
			
			var mockStore = new Mock<IUserStore<ApplicationUser>>();
			_mockUserManager = new Mock<UserManager<ApplicationUser>>(mockStore.Object, null, null, null, null, null, null, null, null);
			_mockUserManager
				.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
				.ReturnsAsync(dummyUser);

			var userid = Guid.NewGuid().ToString();

			_mockTransactionManager = new Mock<ITransactionManager>();
			_mockTransactionManager
				.Setup(x => x.GetTransactions(It.IsAny<string>()))
				.ReturnsAsync(new List<Transaction>
				{
					new Transaction
					{
						Amount = 200.00m,
						BalanceBefore = 1_000.00m,
						BalanceAfter = 1_200.00m,
						CreatedById = Guid.NewGuid().ToString(),
						UserId = userid,
						TransactionType = TransactionType.Deposit
					},
					new Transaction
					{
						Amount = 500.00m,
						BalanceBefore = 1_200.00m,
						BalanceAfter = 700.00m,
						CreatedById = Guid.NewGuid().ToString(),
						UserId = userid,
						TransactionType = TransactionType.Withdraw
					}
				});

			_controller = new BankingController(_mockTransactionManager.Object, _mockUserManager.Object);
		}

		#region AccountDetails Tests

		[Fact]
		public async Task ReturnViewForAccountDetails()
		{
			IActionResult result = await _controller.AccountDetails();

			Assert.IsType<ViewResult>(result);
		}

		[Fact]
		public async Task HaveAccountDetailsListViewModelForAccountDetails()
		{
			IActionResult result = await _controller.AccountDetails();

			ViewResult viewResult = Assert.IsType<ViewResult>(result);
			Assert.IsType<AccountDetailsListViewModel>(viewResult.Model);
		}

		#endregion

		#region Deposit Tests

		[Fact]
		public async Task ReturnViewForGetDeposit()
		{
			IActionResult result = await _controller.Deposit();

			Assert.IsType<ViewResult>(result);
		}

		[Fact]
		public async Task HaveWithdrawDepositViewModelForGetDeposit()
		{
			IActionResult result = await _controller.Deposit();

			ViewResult viewResult = Assert.IsType<ViewResult>(result);
			Assert.IsType<WithdrawDepositViewModel>(viewResult.Model);
		}
		
		[Fact]
		public async Task GoBackToViewIfModelInvalidForPostDeposit()
		{
			WithdrawDepositViewModel model = new WithdrawDepositViewModel() { Amount = 0.00m };
			if (model.Amount < 1)
			{
				_controller.ModelState.AddModelError("Amount", "Please enter an amount.");
			}

			IActionResult result = await _controller.Deposit(model);

			ViewResult viewResult = Assert.IsType<ViewResult>(result);
			Assert.IsType<WithdrawDepositViewModel>(viewResult.Model);
			Assert.Null(viewResult.ViewName);
		}

		[Fact]
		public async Task ShowTransactionSummaryViewOnSuccessForPostDeposit()
		{
			_mockTransactionManager
				.Setup(x => x.SaveFunds(TransactionType.Deposit, It.IsAny<WithdrawDepositViewModel>(), It.IsAny<ApplicationUser>()))
				.ReturnsAsync(new Transaction());

			WithdrawDepositViewModel model = new WithdrawDepositViewModel() { Amount = 1.00m };
			if (model.Amount < 1)
			{
				_controller.ModelState.AddModelError("Amount", "Please enter an amount.");
			}

			IActionResult result = await _controller.Deposit(model);

			ViewResult viewResult = Assert.IsType<ViewResult>(result);
			Assert.IsType<Transaction>(viewResult.Model);
			Assert.Equal("TransactionSummary", viewResult.ViewName);
		}

		[Fact]
		public async Task RedirectToErrorActionOnExceptionForPostDeposit()
		{
			_mockTransactionManager
				.Setup(x => x.SaveFunds(TransactionType.Deposit, It.IsAny<WithdrawDepositViewModel>(), It.IsAny<ApplicationUser>()))
				.ThrowsAsync(new Exception("An error has occured."));

			WithdrawDepositViewModel model = new WithdrawDepositViewModel() { Amount = 1.00m };

			IActionResult result = await _controller.Deposit(model);

			var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
			Assert.Null(redirectToActionResult.ControllerName);
			Assert.Equal("Error", redirectToActionResult.ActionName);
		}

		#endregion

		#region Withdraw Tests

		[Fact]
		public async Task ReturnViewForGetWithdraw()
		{
			IActionResult result = await _controller.Withdraw();

			Assert.IsType<ViewResult>(result);
		}

		[Fact]
		public async Task HaveWithdrawDepositViewModelForGetWithdraw()
		{
			IActionResult result = await _controller.Withdraw();

			ViewResult viewResult = Assert.IsType<ViewResult>(result);
			Assert.IsType<WithdrawDepositViewModel>(viewResult.Model);
		}
		
		[Fact]
		public async Task GoBackToViewIfModelInvalidForPostWithdraw()
		{
			WithdrawDepositViewModel model = new WithdrawDepositViewModel() { Amount = 0.00m };
			if (model.Amount < 1)
			{
				_controller.ModelState.AddModelError("Amount", "Please enter an amount.");
			}

			IActionResult result = await _controller.Withdraw(model);

			ViewResult viewResult = Assert.IsType<ViewResult>(result);
			Assert.IsType<WithdrawDepositViewModel>(viewResult.Model);
			Assert.Null(viewResult.ViewName);
		}

		[Fact]
		public async Task ShowTransactionSummaryViewOnSuccessForPostWithdraw()
		{
			_mockTransactionManager
				.Setup(x => x.SaveFunds(TransactionType.Withdraw, It.IsAny<WithdrawDepositViewModel>(), It.IsAny<ApplicationUser>()))
				.ReturnsAsync(new Transaction());

			WithdrawDepositViewModel model = new WithdrawDepositViewModel() { Amount = 1.00m };
			if (model.Amount < 1)
			{
				_controller.ModelState.AddModelError("Amount", "Please enter an amount.");
			}

			IActionResult result = await _controller.Withdraw(model);

			ViewResult viewResult = Assert.IsType<ViewResult>(result);
			Assert.IsType<Transaction>(viewResult.Model);
			Assert.Equal("TransactionSummary", viewResult.ViewName);
		}

		[Fact]
		public async Task RedirectToErrorActionOnExceptionForPostWithdraw()
		{
			_mockTransactionManager
				.Setup(x => x.SaveFunds(TransactionType.Withdraw, It.IsAny<WithdrawDepositViewModel>(), It.IsAny<ApplicationUser>()))
				.ThrowsAsync(new Exception("An error has occured."));

			WithdrawDepositViewModel model = new WithdrawDepositViewModel() { Amount = 1.00m };

			IActionResult result = await _controller.Withdraw(model);

			var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
			Assert.Null(redirectToActionResult.ControllerName);
			Assert.Equal("Error", redirectToActionResult.ActionName);
		}

		#endregion

		#region Transfer Tests

		[Fact]
		public async Task ReturnViewForGetTransfer()
		{
			IActionResult result = await _controller.Transfer();

			Assert.IsType<ViewResult>(result);
		}

		[Fact]
		public async Task HaveTransferViewModelForGetTransfer()
		{
			IActionResult result = await _controller.Transfer();

			ViewResult viewResult = Assert.IsType<ViewResult>(result);
			Assert.IsType<TransferViewModel>(viewResult.Model);
		}

		[Fact]
		public async Task GoBackToViewIfModelInvalidForPostTransfer()
		{
			TransferViewModel model = new TransferViewModel() { Amount = 0.00m };
			if (model.Amount < 1)
			{
				_controller.ModelState.AddModelError("Amount", "Please enter an amount.");
			}

			if (String.IsNullOrWhiteSpace(model.AccountNumber))
			{
				_controller.ModelState.AddModelError("AccountNumber", "Please enter the Account Number you want to deposit to.");
			}

			IActionResult result = await _controller.Transfer(model);

			ViewResult viewResult = Assert.IsType<ViewResult>(result);
			Assert.IsType<TransferViewModel>(viewResult.Model);
			Assert.Null(viewResult.ViewName);
		}

		[Fact]
		public async Task ShowTransactionSummaryViewOnSuccessForPostTransfer()
		{
			_mockTransactionManager
				.Setup(x => x.TransferFunds(It.IsAny<TransferViewModel>(), It.IsAny<ApplicationUser>()))
				.ReturnsAsync(new Transaction());

			TransferViewModel model = new TransferViewModel() { Amount = 1.00m, AccountNumber = "00000002" };
			if (model.Amount < 1)
			{
				_controller.ModelState.AddModelError("Amount", "Please enter an amount.");
			}

			if (String.IsNullOrWhiteSpace(model.AccountNumber))
			{
				_controller.ModelState.AddModelError("AccountNumber", "Please enter the Account Number you want to deposit to.");
			}

			IActionResult result = await _controller.Transfer(model);

			ViewResult viewResult = Assert.IsType<ViewResult>(result);
			Assert.IsType<Transaction>(viewResult.Model);
			Assert.Equal("TransactionSummary", viewResult.ViewName);
		}

		[Fact]
		public async Task RedirectToErrorActionOnExceptionForPostTransfer()
		{
			_mockTransactionManager
				.Setup(x => x.TransferFunds(It.IsAny<TransferViewModel>(), It.IsAny<ApplicationUser>()))
				.ThrowsAsync(new Exception("An error has occured."));

			TransferViewModel model = new TransferViewModel() { Amount = 1.00m, AccountNumber = "00000002" };

			IActionResult result = await _controller.Transfer(model);

			var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
			Assert.Null(redirectToActionResult.ControllerName);
			Assert.Equal("Error", redirectToActionResult.ActionName);
		}

		#endregion

		#region TransactionHistory Tests

		[Fact]
		public async Task ReturnViewForTransactionHistory()
		{
			IActionResult result = await _controller.TransactionHistory();

			Assert.IsType<ViewResult>(result);
		}

		[Fact]
		public async Task HaveListOfTransactionsAsModelForTransactionHistory()
		{
			IActionResult result = await _controller.TransactionHistory();

			ViewResult viewResult = Assert.IsType<ViewResult>(result);
			Assert.IsType<List<Transaction>>(viewResult.Model);
		}

		#endregion
	}
}
