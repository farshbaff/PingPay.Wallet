using WalletApi.Enum;
using WalletApi.Models;
using WalletApi.Repositories;

namespace WalletApi.Services;

public class TransactionCreatorService : ITransactionCreatorService
{
    private readonly ITransactionRepository _transactionRepository;

    public TransactionCreatorService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task CreateTransaction(TransactionRequest request, Wallet wallet, decimal transactionAmount)
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

    public async Task CreateLockFundsTransaction(LockFundsTransactionRequest request, Wallet wallet)
    {
        await _transactionRepository.CreateLockFundsTransaction(new LockTransaction
        {
            WalletId = wallet.Id,
            TransactionType = TransactionType.Lock,
            Amount = request.Amount,
            CorrelationId = request.CorrelationId,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            RemainingLockedAmount = request.Amount,
        });
    }
}