using Dapper;
using System.Data;
using WalletApi.Models;

namespace WalletApi.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnection _dbConnection;

    public UserRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<User> GetUserById(long id)
    {
        const string query = "SELECT * FROM users WHERE id = @Id";
        return await _dbConnection.QueryFirstOrDefaultAsync<User>(query, new { Id = id });
    }

    public async Task<User> GetUserByUuid(Guid uuid)
    {
        const string query = "SELECT * FROM users WHERE user_uuid = @UserUuid";
        return await _dbConnection.QueryFirstOrDefaultAsync<User>(query, new { UserUuid = uuid });
    }

    public async Task<User> AddUser(User user)
    {
        const string query = "INSERT INTO users (user_uuid, created_at) VALUES (@UserUuid, @CreatedAt) RETURNING *";
        return await _dbConnection.QueryFirstOrDefaultAsync<User>(query, user);
    }

    public async Task<bool> DeleteUser(long id)
    {
        const string query = "DELETE FROM users WHERE id = @Id";
        var result = await _dbConnection.ExecuteAsync(query, new { Id = id });
        return result > 0;
    }

    public async Task<bool> UpdateUser(User user)
    {
        const string query = "UPDATE users SET user_uuid = @UserUuid, locked = @IsLocked WHERE id = @Id";
        var result = await _dbConnection.ExecuteAsync(query, user);
        return result > 0;
    }

    public async Task<bool> LockUser(long id)
    {
        const string query = "UPDATE users SET locked = true WHERE id = @Id";
        var result = await _dbConnection.ExecuteAsync(query, new { Id = id });
        return result > 0;
    }

    public async Task<bool> UnlockUser(long id)
    {
        const string query = "UPDATE users SET locked = false WHERE id = @Id";
        var result = await _dbConnection.ExecuteAsync(query, new { Id = id });
        return result > 0;
    }
}