using Billing.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace Billing.Infrastructure.Persistence.Repositories;

public class BillingRepository(BillingDbContext dbContext):IBillingRepository
{
    public async Task CreateAsync(BillingAggregate? billing,CancellationToken cancellationToken)
    {
        await dbContext.Billings.AddAsync(billing,cancellationToken);
    }

    public async Task<BillingAggregate?> GetByUserIdAsync(long userId,CancellationToken cancellationToken)
    {
        return await dbContext.Billings.FirstOrDefaultAsync(c=>c.UserId == userId,cancellationToken);
    }
}