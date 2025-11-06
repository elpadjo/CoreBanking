using CoreBanking.Core.Common;
using CoreBanking.Core.Entities;
using CoreBanking.Core.Enums;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Core.Events
{
    public record AccountCreatedEvent(
        AccountId AccountId,
        AccountNumber AccountNumber,
        CustomerId CustomerId,
        AccountType AccountType,
        Money InitialBalance) : DomainEvent;

    public record AccountDebitedEvent(
        AccountId AccountId,
        Money Amount,
        Money NewBalance,
        string Description,
        string Reference) : DomainEvent;

    public record AccountCreditedEvent(
        AccountId AccountId,
        Money Amount,
        Money NewBalance,
        string Description,
        string Reference) : DomainEvent;

    public record AccountStatusChangedEvent(
        AccountId AccountId,
        AccountStatus OldStatus,
        AccountStatus NewStatus,
        string Reason) : DomainEvent;

    public record AccountDeletedEvent(
        AccountId AccountId,
        string DeletedBy,
        string Reason) : DomainEvent;

    public record InsufficientFundsEvent(
        AccountNumber AccountNumber,
        Money RequestedAmount,
        Money AvailableBalance,
        string Operation) : DomainEvent;

    public record MoneyTransferedEvent (
        TransactionId TransactionId, 
        AccountNumber SourceAccountNumber,
        AccountNumber DestinationAccountNumber, 
        Money Amount, 
        string Reference,
        DateTime TransferDate) : DomainEvent;

    public record HoldRemovedEvent(
        AccountId AccountId, 
        Money Amount,
        Money AvailableBalance) : DomainEvent;

    public record HoldPlacedEvent(
        AccountId AccountId,
        Money Amount,
        string Description,
        Money AvailableBalance) : DomainEvent;
}
