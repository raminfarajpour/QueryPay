namespace Billing.Infrastructure.ExternalServices.WalletService.Models;

public record WithdrawRequestModel(decimal Amount,
    string ReferenceId,
    string Description);