using MediatR;
using Wallet.Api.Extensions;
using Wallet.Application;
using Wallet.Application.Commands.CreateWallet;
using Wallet.Domain;
using Wallet.Infrastructure;
using Wallet.ReadModel;
using Wallet.ReadModel.Queries.GetWallet;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureDomain();
builder.Services.ConfigureApplication();
builder.Services.ConfigureInfrastructure(builder.Configuration);
builder.Services.ConfigureReadModel(builder.Configuration);

builder.Services.AddMediatR(c=>c.RegisterServicesFromAssemblies([typeof(CreateWalletCommand).Assembly, typeof(GetWalletQuery).Assembly]));

builder.Services.AddEndpoints();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapEndpoints();
app.Run();
