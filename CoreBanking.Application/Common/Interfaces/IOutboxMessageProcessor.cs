namespace CoreBanking.Application.Common.Interfaces;

public interface IOutboxMessageProcessor
{
    Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken = default);
}
