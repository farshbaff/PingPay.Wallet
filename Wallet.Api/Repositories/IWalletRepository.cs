using WalletApi.Models;
namespace WalletApi.Repositories;

public interface IWalletRepository
{
    Task<Wallet> AddWallet(long userId, decimal amount);
    Task AddWallet(string userUuid, decimal amount);
    Task<Wallet> GetWallet(Guid transactionCorrelationId);
    Task<Wallet> GetWallet(long userId);
    Task<Wallet> GetWallet(string userUuid);
    Task<bool> LockWallet(long id);
    Task<bool> UnlockWallet(long id);
    Task UpdateWallet(Wallet wallet);
    Task UpdateWalletAmount(Wallet wallet);
}