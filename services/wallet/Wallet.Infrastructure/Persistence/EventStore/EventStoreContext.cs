using Microsoft.EntityFrameworkCore;
using Wallet.Infrastructure.Persistence.EventStore.Entities;

namespace Wallet.Infrastructure.Persistence.EventStore;

public class EventStoreContext(DbContextOptions<EventStoreContext> options) : DbContext(options)
{
    public DbSet<EventEntity> Events { get; set; }
    public DbSet<SnapshotEntity> Snapshots { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventStoreContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}