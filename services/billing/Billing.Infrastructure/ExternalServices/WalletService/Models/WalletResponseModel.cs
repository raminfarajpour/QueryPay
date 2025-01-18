namespace Billing.Infrastructure.ExternalServices.WalletService.Models;

public class WalletResponseModel(Guid walletId, long userId, decimal balance, decimal overUsageThreshold)
{
    public WalletResponseModel() : this(default, default, default, default)
    {
    }

    public Guid WalletId { get; } = walletId;
    public long UserId { get; } = userId;
    public decimal Balance { get; } = balance;
    public decimal OverUsageThreshold { get; } = overUsageThreshold;
}