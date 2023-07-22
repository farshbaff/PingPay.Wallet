using System.ComponentModel.DataAnnotations;
using WalletApi.Enum;

namespace WalletApi.Models;

public class TransactionRequest : WalletRequest
{
    public TransactionType TransactionType { get; set; }
}