using System.ComponentModel.DataAnnotations;

namespace WalletApi.Models;

public class Wallet
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public decimal Amount { get; set; }
    public decimal TotalLockedAmount { get; set; }
    public bool Locked { get; set; }
    [ConcurrencyCheck]
    public long RowVersion { get; set; }
    public DateTime CreatedAt { get; set; }
}