using FluentAssertions;
using FluentValidation;
using Moq;
using WalletApi.Enum;
using WalletApi.Exceptions;
using WalletApi.Models;
using WalletApi.Repositories;
using WalletApi.Services;
using WalletApi.Validators;

namespace Wallet.Tests.Services;

public class WalletServiceTests
{
    private readonly IValidator<TransactionRequest> _transactionRequestValidator;
    private readonly IValidator<LockFundsTransactionRequest> _lockFundsTransactionRequestValidator;
    private readonly Mock<ITransactionIdempotentService> _mockTransactionIdempotentService;
    private readonly Mock<IUserCacheService> _mockUserCacheService;
    private readonly Mock<IWalletRepository> _mockWalletRepository;
    private readonly Mock<ITransactionRepository> _mockTransactionRepository;
    private readonly WalletValidatorService _walletValidatorService;
    private readonly WalletService _walletService;

    public WalletServiceTests()
    {
        _transactionRequestValidator = new TransactionRequestValidator();
        _lockFundsTransactionRequestValidator = new LockFundsTransactionRequestValidator();
        _mockTransactionIdempotentService = new Mock<ITransactionIdempotentService>();
        _mockUserCacheService = new Mock<IUserCacheService>();
        _mockWalletRepository = new Mock<IWalletRepository>();
        _mockTransactionRepository = new Mock<ITransactionRepository>();
        _walletValidatorService = new WalletValidatorService();
        _walletService = new WalletService(
            _transactionRequestValidator,
            _lockFundsTransactionRequestValidator,
            _mockTransactionIdempotentService.Object,
            _mockUserCacheService.Object,
            _mockWalletRepository.Object,
            _mockTransactionRepository.Object,
            _walletValidatorService
        );
    }

    [Fact]
    public async Task DepositOrWithdraw_ValidRequest_ReturnsNewBalance()
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
        var user = new User
        {
            Id = 1,
            UserUuid = request.UserUuid,
            Locked = false
        };
        var wallet = new WalletApi.Models.Wallet
        {
            Id = 1,
            UserId = user.Id,
            Amount = 1000,
            Locked = false
        };
        var expectedBalance = 1100;

        _mockTransactionIdempotentService.Setup(x => x.RequestHasAlreadyBeenProcessed(request.CorrelationId, "DepositOrWithdraw"))
                                         .ReturnsAsync(false);
        _mockUserCacheService.Setup(x => x.GetUserByUuid(request.UserUuid))
                             .ReturnsAsync(user);
        _mockWalletRepository.Setup(x => x.GetWallet(user.Id))
                             .ReturnsAsync(wallet);
        _mockTransactionRepository.Setup(x => x.CreateTransaction(It.IsAny<Transaction>()))
                                  .Returns(Task.FromResult((long)1));

        // Act
        var result = await _walletService.DepositOrWithdraw(request);

        // Assert
        result.Should().Be(expectedBalance);

        wallet.Amount.Should().Be(expectedBalance);

        _mockTransactionIdempotentService.Verify(x => x.RequestHasAlreadyBeenProcessed(request.CorrelationId, "DepositOrWithdraw"), Times.Once);
        _mockUserCacheService.Verify(x => x.GetUserByUuid(request.UserUuid), Times.Once);
        _mockWalletRepository.Verify(x => x.GetWallet(user.Id), Times.Once);
        _mockTransactionRepository.Verify(x => x.CreateTransaction(It.IsAny<Transaction>()), Times.Once);
    }

    [Fact]
    public async Task DepositOrWithdraw_RequestHasAlreadyBeenProcessed_ThrowsException()
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

        _mockTransactionIdempotentService.Setup(x => x.RequestHasAlreadyBeenProcessed(request.CorrelationId, "DepositOrWithdraw"))
                                         .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _walletService.DepositOrWithdraw(request));

        _mockTransactionIdempotentService.Verify(x => x.RequestHasAlreadyBeenProcessed(request.CorrelationId, "DepositOrWithdraw"), Times.Once);
    }

    [Fact]
    public async Task DepositOrWithdraw_UserLocked_ThrowsException()
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
        var user = new User
        {
            Id = 1,
            UserUuid = request.UserUuid,
            Locked = true
        };

        _mockTransactionIdempotentService.Setup(x => x.RequestHasAlreadyBeenProcessed(request.CorrelationId, "DepositOrWithdraw"))
                                         .ReturnsAsync(false);
        _mockUserCacheService.Setup(x => x.GetUserByUuid(request.UserUuid))
                             .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UserIsLockedException>(() => _walletService.DepositOrWithdraw(request));

        _mockTransactionIdempotentService.Verify(x => x.RequestHasAlreadyBeenProcessed(request.CorrelationId, "DepositOrWithdraw"), Times.Once);
        _mockUserCacheService.Verify(x => x.GetUserByUuid(request.UserUuid), Times.Once);
    }

    [Fact]
    public async Task DepositOrWithdraw_WalletLocked_ThrowsException()
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
        var user = new User
        {
            Id = 1,
            UserUuid = request.UserUuid,
            Locked = false
        };
        var wallet = new WalletApi.Models.Wallet
        {
            Id = 1,
            UserId = user.Id,
            Amount = 1000,
            Locked = true
        };

        _mockTransactionIdempotentService.Setup(x => x.RequestHasAlreadyBeenProcessed(request.CorrelationId, "DepositOrWithdraw"))
                                         .ReturnsAsync(false);
        _mockUserCacheService.Setup(x => x.GetUserByUuid(request.UserUuid))
                             .ReturnsAsync(user);
        _mockWalletRepository.Setup(x => x.GetWallet(user.Id))
                             .ReturnsAsync(wallet);

        // Act & Assert
        await Assert.ThrowsAsync<WalletIsLockedException>(() => _walletService.DepositOrWithdraw(request));

        wallet.Amount.Should().Be(1000);

        _mockTransactionIdempotentService.Verify(x => x.RequestHasAlreadyBeenProcessed(request.CorrelationId, "DepositOrWithdraw"), Times.Once);
        _mockUserCacheService.Verify(x => x.GetUserByUuid(request.UserUuid), Times.Once);
        _mockWalletRepository.Verify(x => x.GetWallet(user.Id), Times.Once);
    }

    [Fact]
    public async Task GetTransactions_ValidFilter_ReturnsTransactions()
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

        _mockTransactionRepository.Setup(x => x.GetTransactions(filter)).ReturnsAsync(transactions);

        // Act
        var result = await _walletService.GetTransactions(filter);

        // Assert
        result.Should().BeEquivalentTo(transactions);

        _mockTransactionRepository.Verify(x => x.GetTransactions(filter), Times.Once);
    }
}