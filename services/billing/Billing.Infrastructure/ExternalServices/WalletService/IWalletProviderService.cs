namespace Billing.Infrastructure.ExternalServices.WalletService;

public interface IWalletProviderService
{
    Task<WalletResponseModel> GetUserWalletAsync(Guid walletId,CancellationToken cancellationToken);
    Task<WalletWithdrawResponse> WithdrawAsync(Guid walletId,WithdrawRequestModel withdrawRequest,CancellationToken cancellationToken);
}