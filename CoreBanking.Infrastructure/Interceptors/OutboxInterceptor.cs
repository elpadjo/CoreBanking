using CoreBanking.Core.Common;
using CoreBanking.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;
using System.Reflection;
using CoreBanking.Infrastructure.Extensions;

namespace CoreBanking.Infrastructure.Interceptors
{
    public class OutboxInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not DbContext context)
                return await base.SavingChangesAsync(eventData, result, cancellationToken);

            // Find all entities that inherit AggregateRoot<T>
            var aggregateRootEntries = context.ChangeTracker
                .Entries()
                .Where(e => e.Entity.GetType().GetTypeInfo().IsSubclassOfRawGeneric(typeof(AggregateRoot<>)))
                .ToList();

            // Collect all domain events
            var domainEvents = aggregateRootEntries
                .SelectMany(e => ((dynamic)e.Entity).DomainEvents as IEnumerable<object>)
                .ToList();

            // Convert domain events to outbox messages
            foreach (var domainEvent in domainEvents)
            {
                context.Set<OutboxMessage>().Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = domainEvent.GetType().Name,
                    Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    OccurredOn = ((dynamic)domainEvent).OccurredOn
                });
            }

            // Clear domain events from aggregates
            foreach (var entry in aggregateRootEntries)
            {
                ((dynamic)entry.Entity).ClearDomainEvents();
            }

            // Proceed with saving changes (entities + outbox messages in one transaction)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
