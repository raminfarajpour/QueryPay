using Billing.Api.GrpcServices;
using Billing.Application;
using Billing.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.ConfigureApplication(builder.Configuration);
builder.Services.ConfigureInfrastructure(builder.Configuration);
builder.Services.AddGrpc();


var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGrpcService<BillingGrpcService>();

app.Run();

