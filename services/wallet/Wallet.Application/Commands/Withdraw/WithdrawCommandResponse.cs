namespace Wallet.Application.Commands.Withdraw;

public record WithdrawCommandResponse(decimal Balance, string TransactionId);