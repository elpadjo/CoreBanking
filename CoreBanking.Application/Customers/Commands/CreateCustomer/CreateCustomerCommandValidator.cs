using CoreBanking.Application.Customers.Commands.CreateCustomer;
using FluentValidation;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(c => c.FirstName)
            .NotEmpty().WithMessage("First Name cannot be empty")
            .MaximumLength(50);

        RuleFor(c => c.LastName)
            .NotEmpty().WithMessage("First Name cannot be empty")
            .MaximumLength(50);

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email cannot be empty.")
            .EmailAddress().WithMessage("Email must be valid.");

        RuleFor(c => c.PhoneNumber)
            .NotEmpty().WithMessage("Phone Number cannot be empty")
            .Matches(@"^\+?\d{10,15}$").WithMessage("Phone number must be valid.");

        RuleFor(c => c.Street)
            .NotEmpty().WithMessage("Street cannot be empty");

        RuleFor(c => c.City)
            .NotEmpty().WithMessage("City cannot be empty");

        RuleFor(c => c.State)
            .NotEmpty().WithMessage("State cannot be empty");

        RuleFor(c => c.ZipCode)
            .NotEmpty().WithMessage("ZipCode cannot be empty");

        RuleFor(c => c.Country)
            .NotEmpty().WithMessage("Country cannot be empty")
            .Length(2).WithMessage("Country must be a 2-letter code.");

        RuleFor(c => c.BVN)
            .NotEmpty().WithMessage("BVN cannot be empty")
            .Matches(@"^\d{11}$").WithMessage("BVN must be 11 digits.");

        RuleFor(c => c.DateOfBirth)
            .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past.");

        RuleFor(c => c.CreditScore)
            .InclusiveBetween(300, 850).WithMessage("Credit score must be between 300 and 850.");
    }
}
