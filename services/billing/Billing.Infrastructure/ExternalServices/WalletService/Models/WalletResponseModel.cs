namespace Billing.Infrastructure.ExternalServices.WalletService.Models;

public class WalletResponseModel
{
    public Guid WalletId { get; set;} 
    public long UserId { get; set;} 
    public decimal Balance { get;set; } 
    public decimal OverUsageThreshold { get; set;} 
}