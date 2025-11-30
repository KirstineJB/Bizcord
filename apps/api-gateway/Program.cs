using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Loader Ocelot konfiguration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Registrer Ocelot services
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

await app.UseOcelot();

app.Run();