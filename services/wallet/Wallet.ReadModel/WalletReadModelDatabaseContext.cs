using MongoDB.Driver;
using Wallet.Application.Commands.CreateWallet;
using Wallet.ReadModel.ReadModels;

namespace Wallet.ReadModel;

public class WalletReadModelDatabaseContext
{
    private readonly IMongoDatabase _database;

    public WalletReadModelDatabaseContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
        CreateIndexes();
    }

    public IMongoCollection<WalletReadModel> Wallets => _database.GetCollection<WalletReadModel>("Wallets");
    
    private void CreateIndexes()
    {
        var indexKeysDefinition = Builders<WalletReadModel>.IndexKeys.Ascending(w => w.Id);
        var indexModel = new CreateIndexModel<WalletReadModel>(indexKeysDefinition);
        Wallets.Indexes.CreateOne(indexModel);
    }
}