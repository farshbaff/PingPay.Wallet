using WalletApi.Exceptions;
using WalletApi.Models;

namespace WalletApi.Services
{
    public class WalletValidatorService
    {
        public void ValidateUserExistsAndIsNotLocked(User user, WalletRequest request)
        {
            if (user == null)
            {
                throw new UserNotFoundException($"User {request.UserUuid} not found");
            }

            if (user.Locked)
            {
                throw new UserIsLockedException($"User {user.UserUuid} is locked");
            }
        }

        public void ValidateWalletExistsAndIsNotLocked(Wallet wallet, WalletRequest request)
        {
            if (wallet == null)
            {
                throw new WalletNotFoundException($"Wallet for user {request.UserUuid} not found!");
            }

            if (wallet.Locked)
            {
                throw new WalletIsLockedException($"Wallet for user {request.UserUuid} is locked");
            }
        }

        public void ValidateSufficientFunds(Wallet wallet, decimal transactionAmount)
        {
            if (wallet.Amount - wallet.TotalLockedAmount < transactionAmount)
            {
                throw new InsufficientFundsException();
            }
        }
    }
}