using ProfileService.Api.Messaging;
using ProfileService.Api.vault;
using ProfileService.Application;
using ProfileService.Infrastructure;

// --- Vault ---
var vaultSettings = new VaultSettings
{
    Address = "http://localhost:8200",
    Token = Environment.GetEnvironmentVariable("VAULT_DEV_TOKEN")
            ?? "hvs.tsDlwsO5KMzsIVqxd1bUtWaw"
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