using MongoDB.Driver;
using Wallet.ReadModel.ReadModels;

namespace Wallet.ReadModel.Repositories;

public class WalletReadModelRepository(WalletReadModelDatabaseContext context) : IWalletReadModelRepository
{
    private readonly IMongoCollection<WalletReadModel> _walletsCollection = context.Wallets;

    public async Task SaveAsync(WalletReadModel wallet, CancellationToken cancellationToken)
    {
        await _walletsCollection.InsertOneAsync(wallet, cancellationToken: cancellationToken);
    }
    
    public async Task<WalletReadModel> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _walletsCollection.Find(wallet => wallet.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<WalletReadModel>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _walletsCollection.Find(_ => true).ToListAsync(cancellationToken);
    }

    public async Task<bool> ReplaceWalletAsync(WalletReadModel updatedWallet, CancellationToken cancellationToken)
    {
        var filter = Builders<WalletReadModel>.Filter.Eq(w => w.Id, updatedWallet.Id);
        var result =
            await _walletsCollection.ReplaceOneAsync(filter, updatedWallet, cancellationToken: cancellationToken);

        return result.ModifiedCount > 0;
    }
}