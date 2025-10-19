using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ProfileService.Api.Messaging;
using ProfileService.IntegrationTests.TestDoubles;

namespace ProfileService.IntegrationTests.Support;

public class ProfilesApiFactory : WebApplicationFactory<Program>
{
    public FakeMessageClient FakeBus { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder) 
    {
        builder.ConfigureServices(services =>
        {
           
            var existing = services.SingleOrDefault(s => s.ServiceType == typeof(IMessageClient));
            if (existing is not null) services.Remove(existing);
            services.AddSingleton<IMessageClient>(FakeBus);
        });
    }
}