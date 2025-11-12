using CoreBanking.Core.Common;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Core.Events
{
    public record CustomerCreatedEvent(
        CustomerId CustomerId,
        string FirstName,
        string LastName,
        string Email,
        DateTime DateOfBirth,
        int CreditScore) : DomainEvent;

    public record CustomerProfileUpdatedEvent(
        CustomerId CustomerId,
        ContactInfo OldContactInfo,
        ContactInfo NewContactInfo) : DomainEvent;

    public record CustomerCreditScoreChangedEvent(
        CustomerId CustomerId,
        int OldCreditScore,
        int NewCreditScore,
        string Reason) : DomainEvent;

    public record CustomerStatusChangedEvent(
        CustomerId CustomerId,
        bool WasActive,
        bool IsNowActive,
        string Reason) : DomainEvent;

    public record CustomerDeletedEvent(
        CustomerId CustomerId,
        string DeletedBy,
        string Reason) : DomainEvent;
}