using ProfileService.Api.Handlers;
using ProfileService.Api.Messaging;
using ProfileService.Api.Sagas;
using ProfileService.Api.vault;
using ProfileService.Application;
using ProfileService.Contracts;
using ProfileService.Contracts.Messages;
using ProfileService.Infrastructure;
using Rebus.Config;
using Rebus.Persistence.InMem;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;
using Rebus.Transport.InMem;

// --- Vault ---
var vaultSettings = new VaultSettings
{
    Address = "http://localhost:8200",
    Token = Environment.GetEnvironmentVariable("VAULT_DEV_TOKEN")
            ?? "xxx" //Secret token
};

var vault = new VaultHelper(vaultSettings);

var messagingConnectionString = vault.GetRabbitMqConnectionStringAsync()
    .GetAwaiter()
    .GetResult();


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Configuration["Messaging:ConnectionString"] = messagingConnectionString;


builder.Services.AddMessageClient(builder.Configuration);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddRebus(configure => configure
    
    .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "profiles-saga-queue"))

   
    .Routing(r => r.TypeBased()
        .MapAssemblyOf<UpgradeUserToPremium>("profiles-saga-queue"))

    
    .Sagas(s => s.StoreInMemory())
);

builder.Services.AutoRegisterHandlersFromAssemblyOf<UserUpgradeSaga>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program { }