namespace WalletApi.Exceptions
{
    public class UserIsLockedException: Exception
    {
        public UserIsLockedException(string message) : base(message) { }
    }
}
