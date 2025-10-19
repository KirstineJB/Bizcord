using Microsoft.Extensions.DependencyInjection;
using ProfileService.Application.Contracts;


namespace ProfileService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserProfileService, UserProfileService>();
        return services;
    }
}