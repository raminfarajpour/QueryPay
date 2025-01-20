using Wallet.Application.Commands.CreateWallet;

namespace Wallet.Api.Endpoints.CreateWallet;

public class CreateWalletRequest
{
    public CreateWalletRequest()
    {
        
    }
    public CreateWalletRequest(
        long ownerUserId,
        string ownerMobile,
        decimal initialBalance,
        decimal overUsedThreshold)
    {
        OverUsedThreshold = overUsedThreshold;
        OwnerUserId = ownerUserId;
        OwnerMobile = ownerMobile;
        InitialBalance = initialBalance;
    }
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

    public long OwnerUserId { get; init; }
    public string OwnerMobile { get; init; } 
    public decimal InitialBalance { get; init; } 
    public decimal OverUsedThreshold { get; init; } 
    
}