using System.Security.Cryptography.X509Certificates;
using Billing.Api.GrpcServices;
using Billing.Application;
using Billing.Application.Commands.CreateBilling;
using Billing.Application.Queries.GetWallet;
using Billing.Infrastructure;
using Billing.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services.AddOpenApi();
builder.Services.ConfigureApplication(builder.Configuration);
builder.Services.ConfigureInfrastructure(builder.Configuration);
builder.Services.AddGrpc();

builder.WebHost.ConfigureKestrel(options =>
{
    var certPath = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path");
    var certPassword = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password");

    if (!string.IsNullOrEmpty(certPath) && !string.IsNullOrEmpty(certPassword))
    {
        options.ConfigureHttpsDefaults(httpsOptions =>
        {
            httpsOptions.ServerCertificate = new X509Certificate2(certPath, certPassword);
        });
    }

    options.ListenAnyIP(5161, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
        listenOptions.UseHttps();
    });
});

var app = builder.Build();


await ApplyMigrations(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGrpcService<BillingGrpcService>();

app.Run();

static async Task ApplyMigrations(IServiceProvider serviceProvider)
{
    try
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        await dbContext.Database.MigrateAsync();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var billing = await dbContext.Billings.FirstOrDefaultAsync(c => c.UserId == 111222333, CancellationToken.None);
        if (billing is null)
            await mediator.Send(new CreateBillingCommand(111222333, "09395805855", 50000000, 1000));
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