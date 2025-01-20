namespace Billing.Infrastructure.ExternalServices.WalletService.Models;

public record CreateWalletRequestModel(long OwnerUserId,
    string OwnerMobile,
    decimal InitialBalance,
    decimal OverUsedThreshold);