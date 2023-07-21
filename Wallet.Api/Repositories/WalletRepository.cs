using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using WalletApi.Models;
namespace WalletApi.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly IDbConnection _dbConnection;

    public WalletRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<Wallet> GetWallet(long userId)
    {
        const string sql = @"
            SELECT a.*
            FROM wallet a 
            WHERE user_id = @UserId
        ";

        var wallet =
            await _dbConnection.QuerySingleOrDefaultAsync<Wallet>(sql, new { UserId = userId });
        return wallet;
    }

    public async Task<Wallet> GetWallet(string userUuid)
    {
        const string sql = @"
            SELECT a.*
            FROM wallet a 
                JOIN users u  ON a.user_id = u.id
            WHERE u.user_uuid = @UserUuid
        ";

        var wallet =
            await _dbConnection.QuerySingleOrDefaultAsync<Wallet>(sql,
                new { UserUuid = userUuid });
        return wallet;
    }

    public async Task<Wallet> GetWallet(Guid transactionCorrelationId)
    {
        const string sql = @"
            SELECT a.*
            FROM wallet a 
                JOIN transaction t ON a.id = t.wallet_id
            WHERE t.correlation_id = @CorrelationId
        ";

        var wallet =
            await _dbConnection.QuerySingleOrDefaultAsync<Wallet>(sql,
                new { CorrelationId = transactionCorrelationId });
        return wallet;
    }

    public async Task<Wallet> AddWallet(long userId, decimal amount)
    {
        const string sql = @"
            INSERT INTO wallet (user_id, amount) 
            VALUES (@UserUuid, @CreatedAt) 
            RETURNING *;
        ";
        var user = await _dbConnection.QueryFirstOrDefaultAsync<Wallet>(sql, new { Amount = amount });

        if (user is null)
        {
            throw new ArgumentException("User id is wrong!");
        }

        return user;
    }

    public async Task AddWallet(string userUuid, decimal amount)
    {
        const string sql = @"
            INSERT INTO wallet (user_id, amount)
            SELECT u.id, @Amount
            FROM wallet a 
                JOIN users u  ON a.user_id = u.id
            WHERE u.user_uuid = @UserUuid
        ";

        var rowsAdded = await _dbConnection.ExecuteAsync(sql, new { Amount = amount });
        if (rowsAdded == 0)
        {
            throw new ArgumentException("User uuid is wrong!");
        }
    }

    public async Task UpdateWallet(Wallet wallet)
    {
        const string sql = @"
            UPDATE wallet
            SET user_id = @UserId,
                amount = @Amount,
                total_locked_amount = @TotalLockedAmount,
                locked = @Locked
            WHERE id = @Id AND row_version = @RowVersion
        ";

        var rowsUpdated = await _dbConnection.ExecuteAsync(sql, wallet);
        if (rowsUpdated == 0)
        {
            throw new DbUpdateConcurrencyException("Failed to update wallet due to concurrency conflict");
        }
    }

    public async Task UpdateWalletAmount(Wallet wallet)
    {
        const string sql = @"
            UPDATE wallet
            SET amount = @Amount
            WHERE id = @Id AND row_version = @RowVersion
        ";

        var rowsUpdated = await _dbConnection.ExecuteAsync(sql, wallet);
        if (rowsUpdated == 0)
        {
            throw new DbUpdateConcurrencyException("Failed to update wallet amount and total locked amount due to concurrency conflict");
        }
    }

    public async Task<bool> LockWallet(long id)
    {
        const string query = "UPDATE wallet SET locked = true WHERE id = @Id";
        var result = await _dbConnection.ExecuteAsync(query, new { Id = id });
        return result > 0;
    }

    public async Task<bool> UnlockWallet(long id)
    {
        const string query = "UPDATE wallet SET locked = false WHERE id = @Id";
        var result = await _dbConnection.ExecuteAsync(query, new { Id = id });
        return result > 0;
    }
}