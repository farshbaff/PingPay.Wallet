using System.ComponentModel.DataAnnotations;

namespace WalletApi.Models;

public class LockTransaction : Transaction
{
    public long TransactionId { get; set; }
    public decimal RemainingLockedAmount { get; set; }
    [ConcurrencyCheck]
    public long RowVersion { get; set; }
}