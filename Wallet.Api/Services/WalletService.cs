using FluentValidation;
using System.Transactions;
using WalletApi.Enum;
using WalletApi.Models;
using WalletApi.Repositories;
using Transaction = WalletApi.Models.Transaction;

namespace WalletApi.Services;

public class WalletService : IWalletService
{
    private readonly IValidator<TransactionRequest> _transactionRequestValidator;
    private readonly IValidator<LockFundsTransactionRequest> _lockFundsTransactionRequestValidator;
    private readonly ITransactionIdempotentService _transactionIdempotentService;
    private readonly IUserCacheService _userCacheService;
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly WalletValidatorService _walletValidatorService;
    private readonly ITransactionCreatorService _transactionCreatorService; 

    public WalletService(IValidator<TransactionRequest> transactionRequestValidator, 
        IValidator<LockFundsTransactionRequest> lockFundsTransactionRequestValidator, ITransactionIdempotentService transactionIdempotentService,
        IUserCacheService userCacheService, IWalletRepository walletRepository,
        ITransactionRepository transactionRepository, WalletValidatorService walletValidatorService,
        ITransactionCreatorService transactionCreatorService)
    {
        _transactionRequestValidator = transactionRequestValidator;
        _lockFundsTransactionRequestValidator = lockFundsTransactionRequestValidator;
        _transactionIdempotentService = transactionIdempotentService;
        _userCacheService = userCacheService;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _walletValidatorService = walletValidatorService;
        _transactionCreatorService = transactionCreatorService;
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
            _walletValidatorService.ValidateSufficientFunds(wallet, request.Amount);

        var transactionAmount = 
            request.TransactionType == TransactionType.Deposit ? request.Amount : -request.Amount;

        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            wallet.Amount += transactionAmount;

            await _walletRepository.UpdateWalletAmount(wallet);

            await _transactionCreatorService.CreateTransaction(request, wallet, transactionAmount);

            transaction.Complete();
        }

        return wallet.Amount;
    }

    public async Task<decimal> LockFunds(LockFundsTransactionRequest request)
    {
        await _lockFundsTransactionRequestValidator.ValidateAndThrowAsync(request);

        if (await _transactionIdempotentService.RequestHasAlreadyBeenProcessed(request.CorrelationId, nameof(LockFunds)))
            throw new Exception($"Transaction request with CorrelationId = {request.CorrelationId} has already been processed!");

        var user = await _userCacheService.GetUserByUuid(request.UserUuid);

        _walletValidatorService.ValidateUserExistsAndIsNotLocked(user, request);

        var wallet = await _walletRepository.GetWallet(user.Id);

        _walletValidatorService.ValidateWalletExistsAndIsNotLocked(wallet, request);

        _walletValidatorService.ValidateSufficientFunds(wallet, request.Amount);

        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            wallet.TotalLockedAmount += request.Amount;

            await _walletRepository.UpdateWalletTotalLockedAmount(wallet);

            await _transactionCreatorService.CreateLockFundsTransaction(request, wallet);

            transaction.Complete();
        }

        return wallet.TotalLockedAmount;
    }

    public async Task<IEnumerable<Transaction>> GetTransactions(TransactionFilter filter)
    {
        return await _transactionRepository.GetTransactions(filter);
    }
}