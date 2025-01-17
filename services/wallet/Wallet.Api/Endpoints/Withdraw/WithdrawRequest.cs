using Wallet.Application.Commands.Withdraw;

namespace Wallet.Api.Endpoints.Withdraw;

public record WithdrawRequest(
    decimal Amount,
    string TransactionId,
    string ReferenceId,
    string Description)
{
    public static explicit operator WithdrawCommand(WithdrawRequest request)
    {
        return new WithdrawCommand()
        {
            Amount = request.Amount,
            TransactionId = request.TransactionId,
            ReferenceId = request.ReferenceId,
            Description = request.Description
        };
    }
}