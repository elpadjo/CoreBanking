using CoreBanking.Application.Accounts.Commands.CreateAccount;
using CoreBanking.Core.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Accounts.Commands.CreateTransactions
{
    public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
    {
        public CreateTransactionCommandValidator()
        {
            //RuleFor(x => x.CustomerId.Value)
            //       .NotEmpty().WithMessage("Customer ID is required")
            //       .NotEqual(Guid.Empty).WithMessage("Customer ID cannot be empty");

            //RuleFor(x => x.AccountType)
            //    .NotEmpty().WithMessage("Account type is required")
            //    .Must(BeValidAccountType).WithMessage("Invalid account type. Must be Savings or Current");

            RuleFor(x => x.TrxAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Transaction amount cannot be negative")
                .LessThan(1000000).WithMessage("Transaction amount cannot exceed ₦1,000,000");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .Length(3).WithMessage("Currency must be 3 characters")
                .Must(BeSupportedCurrency).WithMessage("Unsupported currency. Supported: NGN");

            RuleFor(x => x.TransactionType)
                .NotEmpty().WithMessage("TransactionType is required")
                .Must(BeSupportedTransactionType).WithMessage("Unsupported currency. Supported: Credit, Debit, Reversal");
        }

        private bool BeSupportedCurrency(string currency)
            => new[] { "NGN" }.Contains(currency);

        private bool BeSupportedTransactionType(string transactionType)
            => new[] { "credit", "debit", "reversal" }.Contains(transactionType.ToLower());
    }
}
