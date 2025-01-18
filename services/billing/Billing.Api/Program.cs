using Billing.Api.GrpcServices;
using Billing.Application;
using Billing.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;

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
    options.ListenAnyIP(5100, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
        listenOptions.UseHttps(); 
    });
});

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGrpcService<BillingGrpcService>();

app.Run();

