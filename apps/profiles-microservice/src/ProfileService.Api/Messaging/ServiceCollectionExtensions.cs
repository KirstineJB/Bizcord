using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ProfileService.Api.Messaging;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessageClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MessageClientOptions>(configuration.GetSection("Messaging"));

        services.AddSingleton<IBus>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<MessageClientOptions>>().Value;
            if (string.IsNullOrWhiteSpace(opts.ConnectionString))
                throw new InvalidOperationException("Messaging:ConnectionString is missing.");
            return RabbitHutch.CreateBus(opts.ConnectionString);
        });

        services.AddSingleton<IMessageClient, EasyNetQMessageClient>();
        return services;
    }
}