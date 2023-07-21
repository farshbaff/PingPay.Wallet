using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WalletApi.Controllers;
using WalletApi.Enum;
using WalletApi.Models;
using WalletApi.Services;
using Transaction = WalletApi.Models.Transaction;

namespace Wallet.Tests.Controllers;

public class WalletControllerTests
{
    private readonly Mock<IWalletService> _mockWalletService;
    private readonly WalletController _walletController;

    public WalletControllerTests()
    {
        _mockWalletService = new Mock<IWalletService>();
        _walletController = new WalletController(_mockWalletService.Object);
    }

    [Fact]
    public async Task Transaction_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new TransactionRequest
        {
            UserUuid = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            TransactionType = TransactionType.Deposit,
            Amount = 100,
            Description = "Test Deposit"
        };
        var expectedBalance = 200;

        _mockWalletService.Setup(x => x.DepositOrWithdraw(request))
                          .ReturnsAsync(expectedBalance);

        // Act
        var result = await _walletController.Transaction(request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        result.Result.As<OkObjectResult>().Value.Should().Be(expectedBalance);

        _mockWalletService.Verify(x => x.DepositOrWithdraw(request), Times.Once);
    }

    [Fact]
    public async Task Transactions_ValidFilter_ReturnsOk()
    {
        // Arrange
        var filter = new TransactionFilter
        {
            UserUuid = Guid.NewGuid(),
            TransactionType = TransactionType.Deposit.ToString(),
            Date = DateTime.Today,
            StartDate = DateTime.Today.AddDays(-7),
            EndDate = DateTime.Today,
            MinAmount = 10,
            MaxAmount = 100
        };
        var transactions = new List<Transaction>
    {
        new Transaction
        {
            Id = 1,
            WalletId = 1,
            TransactionType = TransactionType.Deposit,
            Amount = 50,
            CorrelationId = Guid.NewGuid(),
            Description = "Test Deposit",
            CreatedAt = DateTime.Today
        },
        new Transaction
        {
            Id = 2,
            WalletId = 1,
            TransactionType = TransactionType.Withdrawal,
            Amount = 25,
            CorrelationId = Guid.NewGuid(),
            Description = "Test Withdrawal",
            CreatedAt = DateTime.Today.AddDays(-1)
        }
    };

        _mockWalletService.Setup(x => x.GetTransactions(filter))
                          .ReturnsAsync(transactions);

        // Act
        var result = await _walletController.Transactions(filter);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        result.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(transactions);

        _mockWalletService.Verify(x => x.GetTransactions(filter), Times.Once);
    }
}
