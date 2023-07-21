namespace WalletApi.Models;

public class User
{
    public long Id { get; set; }
    public Guid UserUuid { get; set; }
    public bool Locked { get; set; }
    public DateTime CreatedAt { get; set; }
}