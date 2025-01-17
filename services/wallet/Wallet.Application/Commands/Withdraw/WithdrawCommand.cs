using MediatR;

namespace Wallet.Application.Commands.Withdraw;

public record WithdrawCommand : IRequest<WithdrawCommandResponse>
{
    public Guid WalletId { get; set; }
    public decimal Amount { get; set; }
    public string ReferenceId { get; set; }
    public string Description { get; set; }
}