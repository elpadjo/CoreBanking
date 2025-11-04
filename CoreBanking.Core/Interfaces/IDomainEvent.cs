// CoreBanking.Core/Common/IDomainEvent.cs
namespace CoreBanking.Core.Common;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}