using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallet.Infrastructure.Persistence.EventStore.Entities;

namespace Wallet.Infrastructure.Persistence.EventStore.Configurations;

public class EventsConfiguration:IEntityTypeConfiguration<EventEntity>
{
    public void Configure(EntityTypeBuilder<EventEntity> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(c=>c.Id);
        builder.Property(c=>c.Index).IsRequired();
        builder.Property(c=>c.AggregateId).IsRequired();
        builder.Property(c=>c.AggregateType).HasMaxLength(300).IsRequired();
        builder.Property(c=>c.Type).HasMaxLength(300).IsRequired();
        builder.Property(c => c.Payload).HasColumnType("jsonb").IsRequired();
        builder.Property(c=>c.Timestamp).IsRequired()
            .HasColumnType("timestamptz");
        
        builder.HasIndex(e=>e.AggregateId);
        builder.HasIndex(e=>e.AggregateType);
        builder.HasIndex(e=>e.Type);
        builder.HasIndex(e=>e.Timestamp);
    }
}