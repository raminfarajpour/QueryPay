namespace Wallet.Infrastructure.Persistence.EventStore.Entities;

public class SnapshotEntity
{
    public SnapshotEntity()
    {
        
    }
    public required Guid Id{ get; set; }
    public required Guid AggregateId{ get; set; }
    public required string AggregateType{ get; set; }
    public required string Type{ get; set; }
    public required string State{ get; set; }
    public required long Version{ get; set; }
    public required DateTimeOffset CreatedAt{ get; set; }
    
}