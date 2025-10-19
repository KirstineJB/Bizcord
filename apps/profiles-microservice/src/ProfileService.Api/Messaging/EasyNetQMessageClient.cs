using EasyNetQ;

namespace ProfileService.Api.Messaging;

public class EasyNetQMessageClient : IMessageClient, IAsyncDisposable
{
    private readonly IBus _bus;

    public EasyNetQMessageClient(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishAsync<T>(T message, string topic, CancellationToken ct = default) where T : class
    {
        await _bus.PubSub.PublishAsync(message, topic, cancellationToken: ct);
    }

    public IDisposable Subscribe<T>(string subscriptionId, string topic, Func<T, Task> handler) where T : class
    {
        return _bus.PubSub.Subscribe<T>(
            subscriptionId,
            async msg => await handler(msg),
            cfg => cfg.WithTopic(topic)
        );
    }

    public async ValueTask DisposeAsync() => _bus.Dispose();
}