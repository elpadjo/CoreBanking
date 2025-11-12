using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand : ICommand<CustomerId>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;

    // Contact info as primitives
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public string Country { get; init; } = "NG";

    public string BVN { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public int CreditScore { get; init; } = 0;
}