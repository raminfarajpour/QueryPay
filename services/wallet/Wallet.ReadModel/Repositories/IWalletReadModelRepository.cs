using Wallet.ReadModel.ReadModels;

namespace Wallet.ReadModel.Repositories;

public interface IWalletReadModelRepository
{
    Task<WalletReadModel> GetByIdAsync(Guid id,CancellationToken cancellationToken );
    Task SaveAsync(WalletReadModel wallet,CancellationToken cancellationToken);
    Task<List<WalletReadModel>> GetAllAsync(CancellationToken cancellationToken);
    Task<bool> ReplaceWalletAsync(WalletReadModel updatedWallet,CancellationToken cancellationToken);

}