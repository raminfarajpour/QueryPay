using Billing.Domain.SeedWorks;

namespace Billing.Domain.Billing;

public class BillingAggregate : AggregateRoot<long>
{
    public long UserId { get; private set; }
    public Guid WalletId { get; private set; }
    public Money Balance { get; private set; }

    public DateTimeOffset CreatedAt { get; init; }

    private readonly List<FinancialItem> _financialItems = [];
    public IReadOnlyCollection<FinancialItem> FinancialItems => [.._financialItems];

    public BillingAggregate(long userId, Guid walletId, Money balance)
    {
        WalletId = walletId;
        UserId = userId;
        CreatedAt = DateTimeOffset.UtcNow;
        Balance = balance;
    }

    public void UpdateBalance(Money balance)
    {
        Balance = balance;
    }

    public void AddFinancialItem(List<CommandType> commands, int affectedRows, PricingSetting pricing)
    {
        var financialItem = FinancialItem.Create(commands, affectedRows, pricing);
        _financialItems.Add(financialItem);
    }
}