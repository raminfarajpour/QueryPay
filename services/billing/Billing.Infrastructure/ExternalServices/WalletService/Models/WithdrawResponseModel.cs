namespace Billing.Infrastructure.ExternalServices.WalletService.Models;

public class WithdrawResponseModel(decimal balance, string transactionId)
{
    public WithdrawResponseModel() : this(default(decimal), null!)
    {
    }

    public decimal Balance { get; } = balance;
    public string TransactionId { get; } = transactionId;
}