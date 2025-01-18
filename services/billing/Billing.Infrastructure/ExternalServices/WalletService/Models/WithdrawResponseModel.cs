namespace Billing.Infrastructure.ExternalServices.WalletService.Models;

public class WithdrawResponseModel
{
    public decimal Balance { get; set; } 
    public string TransactionId { get;set; } 
}