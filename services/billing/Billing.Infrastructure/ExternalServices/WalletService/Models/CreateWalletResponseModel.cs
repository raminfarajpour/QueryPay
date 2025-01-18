namespace Billing.Infrastructure.ExternalServices.WalletService.Models;

public class CreateWalletResponseModel(Guid walletId,decimal balance)
{
    public CreateWalletResponseModel() : this(new Guid(),default(decimal))
    {
        
    }
    public Guid WalletId { get; } = walletId;
    public decimal Balance { get; } = balance;
}