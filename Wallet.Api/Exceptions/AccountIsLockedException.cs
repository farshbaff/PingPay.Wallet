namespace WalletApi.Exceptions;

public class WalletIsLockedException : Exception
{
    public WalletIsLockedException(string message) : base(message) { }
}