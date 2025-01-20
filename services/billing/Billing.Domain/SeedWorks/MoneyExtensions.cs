using Billing.Domain.Billing;

namespace Billing.Domain.SeedWorks;

public static class MoneyExtensions
{
    public static Money Sum<T>(this IEnumerable<T> source, Func<T, Money> selector)
    {
        if (source == null || !source.Any())
            return new Money(0);

        return source.Aggregate(new Money(0), (total, item) => total + selector(item));
    }
    
   
}