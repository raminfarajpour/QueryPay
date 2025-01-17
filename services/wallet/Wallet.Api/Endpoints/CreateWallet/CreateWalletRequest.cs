using Wallet.Application.Commands.CreateWallet;

namespace Wallet.Api.Endpoints.CreateWallet;

public record CreateWalletRequest(
    long OwnerUserId,
    string OwnerMobile,
    decimal InitialBalance,
    decimal OverUsedThreshold)
{
    public static explicit operator CreateWalletCommand(CreateWalletRequest request)
    {
        return new CreateWalletCommand()
        {
            OwnerUserId = request.OwnerUserId,
            OwnerMobile = request.OwnerMobile,
            InitialBalance = request.InitialBalance,
            OverUsedThreshold = request.OverUsedThreshold
        };
    }
}