using WalletApi.Enum;

namespace WalletApi.Models;

public abstract class WalletRequest
{
    public Guid UserUuid { get; set; }
    public decimal Amount { get; set; }
    public Guid CorrelationId { get; set; }
    public string? Description { get; set; }
}