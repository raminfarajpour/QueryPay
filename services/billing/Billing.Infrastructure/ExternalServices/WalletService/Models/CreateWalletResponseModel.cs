namespace Billing.Infrastructure.ExternalServices.WalletService.Models;

public class CreateWalletResponseModel
{
    public Guid WalletId { get; set; } 
    public decimal Balance { get; set;} 
}