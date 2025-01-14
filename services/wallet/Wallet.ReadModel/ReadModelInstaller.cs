using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Wallet.BuildingBlocks.Integration.MessageBroker;
using Wallet.ReadModel.Messages;
using Wallet.ReadModel.Repositories;

namespace Wallet.ReadModel;

public static class ReadModelInstaller
{
    const string WalletDatabaseName = "Wallets";

    public static void ConfigureReadModel(this IServiceCollection services, IConfiguration configuration)
    {
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(BsonType.Binary));

        services.AddSingleton(
            new WalletReadModelDatabaseContext(configuration.GetConnectionString("ReadModel")!, WalletDatabaseName));
        services.AddScoped<IWalletReadModelRepository, WalletReadModelRepository>();
        services.AddSingleton<IMessageHandler, WalletCreatedIntegrationMessageHandler>();
        services.AddSingleton<IMessageHandler, WalletTransactionCreatedIntegrationMessageHandler>();
    }
}