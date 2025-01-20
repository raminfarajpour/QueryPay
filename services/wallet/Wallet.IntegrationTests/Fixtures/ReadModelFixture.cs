using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Testcontainers.RabbitMq;
using Wallet.Application.IntegrationEvents;
using Wallet.BuildingBlocks.Integration.MessageBroker;
using Wallet.Infrastructure.Integration;
using Wallet.ReadModel;
using Wallet.ReadModel.Messages;
using Wallet.ReadModel.Repositories;
using Xunit;

namespace Wallet.IntegrationTests.Fixtures;

public class ReadModelFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _rabbitMqContainer;
    private readonly MongoDbContainer _mongoDbContainer;

    public string DatabaseName { get; private set; } = "test_wallet_db";
    public string ConnectionString => _mongoDbContainer.GetConnectionString();

    public IMongoClient MongoClient { get; private set; }

    public IServiceProvider ServiceProvider { get; private set; }

    public string RabbitMqConnectionString => _rabbitMqContainer.GetConnectionString();

    private readonly EventBusSetting _rabbitMqConnectionSetting = new EventBusSetting
    {
        ConnectionSetting = new RabbitMqConnectionSetting
        {
            Username = "testUser",
            Password = "testPass",
            HostName = "localhost",
            Port = 5675,
            VirtualHost = "/"
        },
        WalletIntegrationEventsExchange = 
            new WalletIntegrationEventsExchangeSetting()
            {
                Name = "test-exchange",
                Type = "direct",
                Durable = true,
                AutoDelete = false,
                WalletCreatedEventQueue = new RabbitMqQueueSetting
                {
                    Name = "test-queue",
                    RoutingKey = "test-routing-key",
                    Durable = true,
                    Exclusive = false,
                    AutoDelete = false,
                    Ttl = 60,
                    RetryQueue = new RabbitMqQueueSetting
                    {
                        Name = "retry-test-queue",
                        RoutingKey = "retry-routing-key",
                        Durable = true,
                        Exclusive = false,
                        AutoDelete = false,
                        Ttl = 120
                    }
                },
                WalletTransactionCreatedEventQueue = new RabbitMqQueueSetting
                {
                    Name = "test-transaction-queue",
                    RoutingKey = "test-transaction-routing-key",
                    Durable = true,
                    Exclusive = false,
                    AutoDelete = false,
                    Ttl = 60,
                    RetryQueue = new RabbitMqQueueSetting
                    {
                        Name = "retry-test-transaction-queue",
                        RoutingKey = "retry-transaction-routing-key",
                        Durable = true,
                        Exclusive = false,
                        AutoDelete = false,
                        Ttl = 120
                    }
                },WalletUpdatedEventQueue = new RabbitMqQueueSetting
                {
                    Name = "test-update-queue",
                    RoutingKey = "test-update-routing-key",
                    Durable = true,
                    Exclusive = false,
                    AutoDelete = false,
                    Ttl = 60,
                    RetryQueue = new RabbitMqQueueSetting
                    {
                        Name = "retry-test-update-queue",
                        RoutingKey = "retry-update-routing-key",
                        Durable = true,
                        Exclusive = false,
                        AutoDelete = false,
                        Ttl = 120
                    }
                }
            }
    }; 
    public ReadModelFixture()
    {
        _mongoDbContainer = new MongoDbBuilder()
            .WithImage("mongo:5.0") 
            .WithUsername("testUser")
            .WithPassword("testPass")
            .WithHostname("localhost")
            .WithName(DatabaseName)
            .WithPortBinding(27017, 27017) 
            .Build();
        
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithHostname(_rabbitMqConnectionSetting.ConnectionSetting.HostName)
            .WithUsername(_rabbitMqConnectionSetting.ConnectionSetting.Username)
            .WithPassword(_rabbitMqConnectionSetting.ConnectionSetting.Password)
            .WithImage("rabbitmq:3.9-management")
            .WithPortBinding(_rabbitMqConnectionSetting.ConnectionSetting.Port, 5672)   
            .WithPortBinding(15672, 15672) 
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _rabbitMqContainer.StartAsync();

        await _mongoDbContainer.StartAsync();

        MongoClient = new MongoClient(ConnectionString);

        var services = new ServiceCollection();
        services.AddSingleton(
            new WalletReadModelDatabaseContext(ConnectionString!, DatabaseName));
        services.AddScoped<IWalletReadModelRepository, WalletReadModelRepository>();
        services.AddSingleton<IMessageHandler, WalletCreatedIntegrationMessageHandler>();
        services.AddSingleton<IMessageHandler, WalletTransactionCreatedIntegrationMessageHandler>();
        services.AddSingleton<IMessageHandler, WalletUpdatedIntegrationMessageHandler>();
        services.AddSingleton<IProducerService, ProducerService>();

        services.AddLogging(configure =>
        {
            configure.ClearProviders(); 
            configure.SetMinimumLevel(LogLevel.Debug); 
        });

        services.AddSingleton<IOptions<EventBusSetting>>(Options.Create(_rabbitMqConnectionSetting));
        services.AddSingleton<IRabbitMqConnectionFactory<EventBusSetting>,RabbitMqConnectionFactory<EventBusSetting>>();

        services.AddSingleton<IRabbitMqConnectionManager, RabbitMqConnectionManager<EventBusSetting>>();

        services.AddSingleton<IConsumerService, ConsumerService>();
        
        ServiceProvider = services.BuildServiceProvider();
    }

    public async Task DisposeAsync()
    {
        await _rabbitMqContainer.StopAsync();
        await _mongoDbContainer.StopAsync();

    }
}