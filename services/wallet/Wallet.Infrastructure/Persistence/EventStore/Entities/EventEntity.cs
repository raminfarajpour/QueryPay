namespace Wallet.Infrastructure.Persistence.EventStore.Entities;

public class EventEntity
{
    public required Guid Id { get; set; } 
    public required Guid AggregateId{ get; set; }
    public required long Index{ get; set; }
    public required string AggregateType{ get; set; }
    public required string Type{ get; set; }
    public required string Payload{ get; set; }
    public required DateTimeOffset Timestamp{ get; set; }
    
}