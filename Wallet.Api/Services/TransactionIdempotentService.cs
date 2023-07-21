using WalletApi.Repositories;

namespace WalletApi.Services;

public class TransactionIdempotentService : ITransactionIdempotentService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<TransactionIdempotentService> _logger;

    public TransactionIdempotentService(ITransactionRepository transactionRepository, ILogger<TransactionIdempotentService> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task<bool> RequestHasAlreadyBeenProcessed(Guid requestCorrelationId, string operation)
    {
        // Check if this request has already been processed
        var existingTransaction = await _transactionRepository.GetTransactionByCorrelationId(requestCorrelationId);
        if (existingTransaction != null)
        {
            _logger.LogInformation($"{operation} request with correlation ID {requestCorrelationId} has already been processed");
            return true;
        }

        return false;
    }
}