namespace WalletApi.Exceptions;

public class InsufficientFundsException : Exception
{
    public InsufficientFundsException() : base("Insufficient funds") { }
}