using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wallet.BuildingBlocks.Integration;
using Wallet.BuildingBlocks.Integration.MessageBroker;
using Wallet.Infrastructure.Integration;
using Wallet.Infrastructure.Persistence.EventStore;
using Wallet.Infrastructure.Persistence.EventStore.Repositories;

namespace Wallet.Infrastructure;

public static class InfrastructureInstaller
{
    public static void ConfigureInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EventStoreContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("EventStore")));
        services.AddScoped<IEventStore, EventStore>();

        var rabbitMqSettingSection = configuration.GetSection(nameof(EventBusSetting));
        services.Configure<EventBusSetting>(rabbitMqSettingSection);

        services.AddSingleton<IProducerService, ProducerService>();

        services.AddSingleton<IRabbitMqConnectionManager,RabbitMqConnectionManager<EventBusSetting>>();

        services.AddSingleton<IConsumerService, ConsumerService>();
        
        services.AddHostedService<ConsumersHostedService>();
    }
    
}