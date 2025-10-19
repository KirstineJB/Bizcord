using Microsoft.Extensions.DependencyInjection;
using ProfileService.Domain.Repositories;
using ProfileService.Infrastructure.Persistence;

namespace ProfileService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IUserProfileRepository, InMemoryUserProfileRepository>();
        return services;
    }
}