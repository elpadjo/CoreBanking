namespace CoreBanking.Application.Common.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchDomainEventsAsync(CancellationToken cancellationToken);
}
