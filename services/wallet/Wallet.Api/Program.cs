using MediatR;
using Microsoft.EntityFrameworkCore;
using Wallet.Api.Extensions;
using Wallet.Application;
using Wallet.Application.Commands.CreateWallet;
using Wallet.Domain;
using Wallet.Infrastructure;
using Wallet.Infrastructure.Persistence.EventStore;
using Wallet.ReadModel;
using Wallet.ReadModel.Queries.GetWallet;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables() 
    .AddCommandLine(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureDomain();
builder.Services.ConfigureApplication();
builder.Services.ConfigureInfrastructure(builder.Configuration);
builder.Services.ConfigureReadModel(builder.Configuration);

builder.Services.AddMediatR(c=>c.RegisterServicesFromAssemblies([typeof(CreateWalletCommand).Assembly, typeof(GetWalletQuery).Assembly]));

builder.Services.AddEndpoints();
var app = builder.Build();

await ApplyMigrations(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapEndpoints();
app.Run();


static async Task ApplyMigrations(IServiceProvider serviceProvider)
{
    try
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventStoreContext>();
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        // Log the error or handle it as needed
        // For example, using a logging framework:
        // var logger = serviceProvider.GetRequiredService<ILogger<Startup>>();
        // logger.LogError(ex, "An error occurred while migrating the database.");

        throw; // Optionally rethrow the exception to prevent the application from starting
    }
}