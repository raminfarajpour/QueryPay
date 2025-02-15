using Billing.Domain.SeedWorks;

namespace Billing.Domain.Billing;

public class FinancialItem:Entity<long>
{
    private FinancialItem()
    {
    }

    private readonly List<CommandType> _commands = [];
    public IReadOnlyCollection<CommandType> Commands => [.._commands];
    public long RecordsAffected { get; private set; }
    public Money Amount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public FinancialItem(List<CommandType> commands, long recordsAffected, Money amount)
    {
        _commands = commands;
        RecordsAffected = recordsAffected;
        Amount = amount;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static FinancialItem Create(List<CommandType> commands, long affectedRows, PricingSetting pricing)
    {
        var amount = commands.Sum(c => pricing.CommandPricing.First(p => p.Type == c).Price);
        amount += new Money(affectedRows * pricing.AffectedRowPrice.Amount);
        
        return new FinancialItem(commands, affectedRows, amount);
    }

}