using WalletApi.Models;

namespace WalletApi.Repositories;

public interface IUserRepository
{
    Task<User> GetUserById(long id);
    Task<User> GetUserByUuid(Guid uuid);
    Task<User> AddUser(User user);
    Task<bool> DeleteUser(long id);
    Task<bool> UpdateUser(User user);
    Task<bool> LockUser(long id);
    Task<bool> UnlockUser(long id);
}
