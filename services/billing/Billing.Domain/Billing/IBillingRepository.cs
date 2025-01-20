namespace Billing.Domain.Billing;

public interface IBillingRepository
{
    Task CreateAsync(BillingAggregate? billing,CancellationToken cancellationToken);
    Task<BillingAggregate?> GetByUserIdAsync(long userId,CancellationToken cancellationToken);
}