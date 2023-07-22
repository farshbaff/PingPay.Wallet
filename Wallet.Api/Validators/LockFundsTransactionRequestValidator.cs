using FluentValidation;
using WalletApi.Models;

namespace WalletApi.Validators;

public class LockFundsTransactionRequestValidator : AbstractValidator<LockFundsTransactionRequest>
{
    public LockFundsTransactionRequestValidator()
    {
        RuleFor(x => x.UserUuid)
            .NotEmpty()
            .WithMessage("Invalid User uuid");

        RuleFor(x => x.Amount)
            .NotNull()
            .GreaterThan(0)
            .WithMessage("Invalid Amount");

        RuleFor(x => x.CorrelationId)
            .NotEmpty()
            .WithMessage("Invalid CorrelationId");
    }
}