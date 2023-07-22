using WalletApi.Models;

namespace WalletApi.Services;

public interface ITransactionCreatorService
{
    Task CreateTransaction(TransactionRequest request, Wallet wallet, decimal transactionAmount);
    Task CreateLockFundsTransaction(LockFundsTransactionRequest request, Wallet wallet);    
}