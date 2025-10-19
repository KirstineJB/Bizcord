using System.Collections.Concurrent;
using ProfileService.Api.Messaging;

namespace ProfileService.IntegrationTests.TestDoubles;

public sealed class FakeMessageClient : IMessageClient
{
    public ConcurrentBag<(object Message, string? Topic)> Published { get; } = new();


    public Task PublishAsync<T>(T message, string? topic = null, CancellationToken ct = default) where T : class
    {
        Published.Add((message!, topic));
        return Task.CompletedTask;
    }



    IDisposable IMessageClient.Subscribe<T>(string subscriptionId, string topic, Func<T, Task> handler)
    {
        return Task.CompletedTask;
    }
}