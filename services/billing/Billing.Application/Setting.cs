using Billing.Domain.Billing;

namespace Billing.Application;

public static class Setting //todo: move to config
{
    public static readonly PricingSetting PricingSetting = new(
        CommandPricing:
        [
            new CommandPricingSetting(CommandType.Select, new Money(30)),
            new CommandPricingSetting(CommandType.Update, new Money(200)),
            new CommandPricingSetting(CommandType.Insert, new Money(100)),
            new CommandPricingSetting(CommandType.Delete, new Money(50))
        ], 
        AffectedRowPrice: new Money(10));
}