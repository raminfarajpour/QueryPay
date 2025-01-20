using Billing.Application.Commands.CreateBilling;
using Billing.Application.MessageHandlers;
using Billing.Infrastructure.SeedWorks.Integration.MessageBroker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Billing.Application;

public static class ApplicationInstaller
{
    public static void ConfigureApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(CreateBillingCommand).Assembly));
        services.AddSingleton<IMessageHandler, UsageMessageHandler>();
    }
}