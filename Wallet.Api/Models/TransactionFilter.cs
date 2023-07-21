namespace WalletApi.Models;

public class TransactionFilter
{
    public Guid? UserUuid { get; set; }

    public string? TransactionType { get; set; }

    public DateTime? Date { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal? MinAmount { get; set; }

    public decimal? MaxAmount { get; set; }
}