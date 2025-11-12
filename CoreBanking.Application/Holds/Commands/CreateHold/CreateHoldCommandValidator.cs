using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Holds.Commands.CreateHold
{
    public class CreateHoldCommandValidator : AbstractValidator<CreateHoldCommand>
    {
        public CreateHoldCommandValidator()
        {

            RuleFor(x => x.AccountId.Value)
              .NotEmpty().WithMessage("Account ID is required")
              .NotEqual(Guid.Empty).WithMessage("Account ID cannot be empty");

            RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Cannot Hold a Negative amount");
        }
    }
}
