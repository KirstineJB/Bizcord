namespace ProfileService.Api.Messaging;


public interface IMessageClient
{
    Task PublishAsync<T>(T message, string topic, CancellationToken ct = default) where T : class;

    IDisposable Subscribe<T>(string subscriptionId, string topic, Func<T, Task> handler) where T : class;
}