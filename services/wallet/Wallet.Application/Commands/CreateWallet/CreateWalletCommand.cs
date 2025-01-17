using MediatR;

namespace Wallet.Application.Commands.CreateWallet;

public record CreateWalletCommand : IRequest<CreateWalletCommandResponse>
{
    public decimal InitialBalance { get; set; }
    public long OwnerUserId { get; set; }
    public string OwnerMobile { get; set; }
    public decimal OverUsedThreshold { get; set; }
}