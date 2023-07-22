using WalletApi.Models;

namespace WalletApi.Services;

public interface ITransactionCreatorService
{
    Task<long> CreateTransaction(TransactionRequest request, Wallet wallet, decimal transactionAmount);
    Task<long> CreateLockFundsTransaction(LockFundsTransactionRequest request, Wallet wallet);    
}