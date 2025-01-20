namespace Billing.Domain.Billing;

public record PricingSetting(List<CommandPricingSetting> CommandPricing, Money AffectedRowPrice);
public record CommandPricingSetting(CommandType Type,Money Price);