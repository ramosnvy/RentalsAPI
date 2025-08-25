namespace Rentals.Application.Abstractions;

public interface IMessageBus
{
    Task PublishAsync<T>(T message, string queue, CancellationToken cancellationToken = default) where T : class;
    Task SubscribeAsync<T>(string queue, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class;
}
