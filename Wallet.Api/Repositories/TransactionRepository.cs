using System.Data;
using System.Text;
using Dapper;
using WalletApi.Models;
using Transaction = WalletApi.Models.Transaction;

namespace WalletApi.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly IDbConnection _dbConnection;

    public TransactionRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<long> CreateTransaction(Transaction transaction)
    {
        const string sql = @"
        INSERT INTO transaction (wallet_id, transaction_type, amount, correlation_id, description)
        VALUES (@WalletId, @TransactionType, @Amount, @CorrelationId, @Description)
        RETURNING id
        ";
        return await _dbConnection.QuerySingleOrDefaultAsync<long>(sql, transaction);
    }

    public async Task<long> CreateLockFundsTransaction(LockTransaction transaction)
    {
        transaction.Id = await CreateTransaction(transaction);

        var lockTransaction = new LockTransaction
        {
            TransactionId = transaction.Id,
            RemainingLockedAmount = transaction.RemainingLockedAmount
        };

        const string lockSql = @"
                INSERT INTO lock_transaction (transaction_id, remaining_locked_amount)
                VALUES (@TransactionId, @RemaininglockedAmount)";

        await _dbConnection.ExecuteAsync(lockSql, lockTransaction);

        return transaction.Id;
    }

    public async Task<Transaction> GetTransaction(long id)
    {
        const string sql = "SELECT * FROM transaction WHERE id = @Id";
        return await _dbConnection.QuerySingleOrDefaultAsync<Transaction>(sql, new { Id = id });
    }

    public async Task<List<Transaction>> GetTransactions(TransactionFilter filter)
    {
        var sql = new StringBuilder();
        sql.Append("SELECT * FROM transaction WHERE 1=1");

        if (filter.UserUuid.HasValue)
        {
            sql.Append(" AND wallet_id IN (SELECT id FROM wallets WHERE user_id = (SELECT id FROM users WHERE user_uuid = @UserUuid))");
        }

        if (!string.IsNullOrEmpty(filter.TransactionType))
        {
            sql.Append(" AND transaction_type = @TransactionType");
        }

        if (filter.Date.HasValue)
        {
            sql.Append(" AND created_at = @Date");
        }

        if (filter.StartDate.HasValue)
        {
            sql.Append(" AND created_at >= @StartDate");
        }

        if (filter.EndDate.HasValue)
        {
            sql.Append(" AND created_at <= @EndDate");
        }

        if (filter.MinAmount.HasValue)
        {
            sql.Append(" AND amount >= @MinAmount");
        }

        if (filter.MaxAmount.HasValue)
        {
            sql.Append(" AND amount <= @MaxAmount");
        }

        return (List<Transaction>)await _dbConnection.QueryAsync<Transaction>(sql.ToString(), filter);
    }

    public async Task<Transaction> GetTransactionByCorrelationId(Guid correlationId)
    {
        const string query = "SELECT * FROM transaction WHERE correlation_id = @CorrelationId";
        return await _dbConnection.QueryFirstOrDefaultAsync<Transaction>(query, new { CorrelationId = correlationId });
    }
}