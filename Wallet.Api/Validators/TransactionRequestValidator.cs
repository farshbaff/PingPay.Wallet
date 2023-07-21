using FluentValidation;
using FluentValidation.Results;
using WalletApi.Enum;
using WalletApi.Models;

namespace WalletApi.Validators;

public class TransactionRequestValidator : AbstractValidator<TransactionRequest>
{
    public TransactionRequestValidator()
    {
        RuleFor(x => x.UserUuid)
            .NotNull()
            .WithMessage("Invalid user uuid");

        RuleFor(x => x.CorrelationId)
            .NotNull()
            .WithMessage("Invalid correlation id");

        RuleFor(x => x.TransactionType)
            .NotNull()
            .WithMessage("Invalid TransactionType");

        RuleFor(x => x.Amount)
            .NotNull()
            .GreaterThan(0)
            .WithMessage("Invalid Amount");
    }
}