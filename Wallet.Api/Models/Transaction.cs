using WalletApi.Enum;

namespace WalletApi.Models;

public class Transaction
{
    public long Id { get; set; }
    public long WalletId { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public Guid CorrelationId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}