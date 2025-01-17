using Billing.Infrastructure.ExternalServices.WalletService.Models;

namespace Billing.Infrastructure.ExternalServices.WalletService;

public interface IWalletProviderService
{
    Task<CreateWalletResponseModel> CreateUserWallet(CreateWalletRequestModel createRequest,CancellationToken cancellationToken);
    Task<WalletResponseModel> GetUserWalletAsync(Guid walletId,CancellationToken cancellationToken);
    Task<WithdrawResponseModel> WithdrawAsync(Guid walletId,WithdrawRequestModel withdrawRequest,CancellationToken cancellationToken);
}

