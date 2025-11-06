using CoreBanking.Core.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreBanking.Core.Common;

public abstract class AggregateRoot<TId> : IAggregateRoot where TId : notnull
{
    public TId Id { get; protected set; }
    public DateTime DateCreated { get; protected set; } = DateTime.UtcNow;
    public DateTime DateUpdated { get; protected set; } = DateTime.UtcNow;

    [NotMapped]
    private readonly List<IDomainEvent> _domainEvents = new();

    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void UpdateTimestamp() => DateUpdated = DateTime.UtcNow;
}
