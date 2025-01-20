using Billing.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace Billing.Infrastructure.Persistence;

public class BillingDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<BillingAggregate?> Billings { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
