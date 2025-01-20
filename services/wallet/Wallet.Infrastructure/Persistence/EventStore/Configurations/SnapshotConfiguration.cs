using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallet.BuildingBlocks.Domain;
using Wallet.Infrastructure.Persistence.EventStore.Entities;

namespace Wallet.Infrastructure.Persistence.EventStore.Configurations;

public class SnapshotConfiguration:IEntityTypeConfiguration<SnapshotEntity>
{
    public void Configure(EntityTypeBuilder<SnapshotEntity> builder)
    {
        builder.ToTable("Snapshots");

        builder.HasKey(c=>c.Id);
        builder.Property(c=>c.Version).IsRequired();
        builder.Property(c=>c.AggregateId).IsRequired();
        builder.Property(c=>c.AggregateType).HasMaxLength(300).IsRequired();
        builder.Property(c=>c.Type).HasMaxLength(300).IsRequired();
        builder.Property(c => c.State).HasColumnType("jsonb").IsRequired();
        builder.Property(c=>c.CreatedAt).IsRequired()
            .HasColumnType("timestamptz");
        
        builder.HasIndex(e=>e.AggregateId);
        builder.HasIndex(e=>e.AggregateType);
        builder.HasIndex(e=>e.Type);
        builder.HasIndex(e=>e.Version);
        builder.HasIndex(e=>e.CreatedAt);
    }
}