namespace WalletApi.Services;

public interface ITransactionIdempotentService
{
    Task<bool> RequestHasAlreadyBeenProcessed(Guid requestCorrelationId, string operation);
}