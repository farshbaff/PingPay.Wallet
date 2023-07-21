using System.ComponentModel.DataAnnotations;
using WalletApi.Enum;

namespace WalletApi.Models;

public class TransactionRequest
{
    public Guid UserUuid { get; set; }
    public Guid CorrelationId { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}