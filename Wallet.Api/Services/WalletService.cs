using FluentValidation;
using System.Transactions;
using WalletApi.Enum;
using WalletApi.Models;
using WalletApi.Repositories;
using WalletApi.Services;
using Transaction = WalletApi.Models.Transaction;

namespace WalletApi.Services;

public class WalletService : IWalletService
{
    private readonly IValidator<TransactionRequest> _transactionRequestValidator;
    private readonly ITransactionIdempotentService _transactionIdempotentService;
    private readonly IUserCacheService _userCacheService;
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly WalletValidatorService _walletValidatorService;

    public WalletService(IValidator<TransactionRequest> transactionRequestValidator, ITransactionIdempotentService transactionIdempotentService,
        IUserCacheService userCacheService, IWalletRepository walletRepository,
        ITransactionRepository transactionRepository, WalletValidatorService walletValidatorService)
    {
        _transactionRequestValidator = transactionRequestValidator;
        _transactionIdempotentService = transactionIdempotentService;
        _userCacheService = userCacheService;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _walletValidatorService = walletValidatorService;
    }

    public async Task<decimal> DepositOrWithdraw(TransactionRequest request)
    {
        await _transactionRequestValidator.ValidateAndThrowAsync(request);

        if (await _transactionIdempotentService.RequestHasAlreadyBeenProcessed(request.CorrelationId, nameof(DepositOrWithdraw)))
            throw new Exception($"Transaction request with CorrelationId = {request.CorrelationId} has already been processed!");

        var user = await _userCacheService.GetUserByUuid(request.UserUuid);

        _walletValidatorService.ValidateUserExistsAndIsNotLocked(user, request);

        var wallet = await _walletRepository.GetWallet(user.Id);

        _walletValidatorService.ValidateWalletExistsAndIsNotLocked(wallet, request);

        if(request.TransactionType == TransactionType.Withdrawal)
            _walletValidatorService.ValidateSufficientFundsForWithydrawal(wallet, request.Amount);

        var transactionAmount = 
            request.TransactionType == TransactionType.Deposit ? request.Amount : -request.Amount;

        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            wallet.Amount += transactionAmount;

            await _walletRepository.UpdateWalletAmount(wallet);

            await CreateTransaction(request, wallet, transactionAmount);

            transaction.Complete();
        }

        return wallet.Amount;
    }
    
    public async Task<IEnumerable<Transaction>> GetTransactions(TransactionFilter filter)
    {
        return await _transactionRepository.GetTransactions(filter);
    }

    private async Task CreateTransaction(TransactionRequest request, Wallet wallet, decimal transactionAmount)
    {
        await _transactionRepository.CreateTransaction(new Transaction
        {
            WalletId = wallet.Id,
            TransactionType = request.TransactionType,
            Amount = transactionAmount,
            CorrelationId = request.CorrelationId,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
        });
    }
}