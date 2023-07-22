using WalletApi.Enum;

namespace WalletApi.Models;

public class LockFundsTransactionRequest : WalletRequest
{
    public TransactionType TransactionType => TransactionType.Lock;
}