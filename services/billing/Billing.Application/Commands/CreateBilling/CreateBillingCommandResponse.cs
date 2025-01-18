namespace Billing.Application.Commands.CreateBilling;

public class CreateBillingCommandResponse(Guid walletId)
{
    public Guid WalletId { get; } = walletId;

    public CreateBillingCommandResponse():this(Guid.NewGuid())
    {
        
    }
}