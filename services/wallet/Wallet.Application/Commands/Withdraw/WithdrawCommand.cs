using MediatR;

namespace Wallet.Application.Commands.Withdraw;

public record WithdrawCommand : IRequest
{
    public Guid WalletId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionId { get; set; }
    public string ReferenceId { get; set; }
    public string Description { get; set; }
}