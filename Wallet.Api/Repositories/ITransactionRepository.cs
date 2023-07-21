using WalletApi.Enum;
using WalletApi.Models;

namespace WalletApi.Repositories;

public interface ITransactionRepository
{
    Task<long> CreateTransaction(Transaction transaction);
    Task<Transaction> GetTransaction(long id);
    Task<List<Transaction>> GetTransactions(TransactionFilter filter);
    Task<Transaction> GetTransactionByCorrelationId(Guid correlationId);
}