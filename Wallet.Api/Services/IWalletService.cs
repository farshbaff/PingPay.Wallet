using WalletApi.Models;

namespace WalletApi.Services;

public interface IWalletService
{
    Task<decimal> DepositOrWithdraw(TransactionRequest request);
    Task<decimal> LockFunds(LockFundsTransactionRequest request);
    Task<IEnumerable<Transaction>> GetTransactions(TransactionFilter filter);
}

