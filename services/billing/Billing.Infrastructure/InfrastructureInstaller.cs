using Billing.Domain.Billing;
using Billing.Infrastructure.ExternalServices;
using Billing.Infrastructure.ExternalServices.SeedWorks;
using Billing.Infrastructure.ExternalServices.WalletService;
using Billing.Infrastructure.Integration;
using Billing.Infrastructure.Persistence;
using Billing.Infrastructure.Persistence.Repositories;
using Billing.Infrastructure.SeedWorks.Integration.MessageBroker;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Billing.Infrastructure;

public static class InfrastructureInstaller
{
    public static void ConfigureInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigureRepositories(services);

        ConfigureWriteDbContext(services, configuration);
        
        ConfigureExternalServices(services, configuration);

        ConfigureRabbitMq(services, configuration);
    }


    private static void ConfigureRabbitMq(IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqSettingSection = configuration.GetSection(nameof(EventBusSetting));
        services.Configure<EventBusSetting>(rabbitMqSettingSection);

        services.AddSingleton<IConsumerService, ConsumerService>();

        services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
        services.AddSingleton<IRabbitMqConnectionManager, RabbitMqConnectionManager>();

        services.AddHostedService<ConsumersHostedService>();
    }

    private static void ConfigureExternalServices(IServiceCollection services, IConfiguration configuration)
    {
        var externalServiceSettingSection = configuration.GetSection(nameof(ExternalServicesSetting));
        services.Configure<ExternalServicesSetting>(externalServiceSettingSection);

        var externalServiceSetting = externalServiceSettingSection.Get<ExternalServicesSetting>();
        services.AddHttpClientByConfig(externalServiceSetting.Wallet);

        services.AddScoped<IWalletProviderService, WalletProviderService>();
    }


    private static void ConfigureWriteDbContext(IServiceCollection services, IConfiguration configuration)
    {
        var connectionStringBuilder =
            new SqlConnectionStringBuilder(configuration.GetConnectionString("BillingDb"))
            {
                ApplicationName = "Billing",
                TrustServerCertificate = true
            };

        services.AddDbContext<BillingDbContext>(options =>
        {
            options
                .UseSqlServer(connectionStringBuilder.ConnectionString,
                    x => { x.MigrationsHistoryTable("BillingMigrationHistory", "dbo"); }
                )
#if DEBUG
                .EnableSensitiveDataLogging()
#endif
                ;
        });
    }

    private static void ConfigureRepositories(IServiceCollection services)
    {
        services.AddScoped<IBillingRepository, BillingRepository>();
    }
}