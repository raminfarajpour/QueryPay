using Billing.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Persistence.Configurations;

public class BillingAggregateConfiguration : IEntityTypeConfiguration<BillingAggregate>
{
    public void Configure(EntityTypeBuilder<BillingAggregate> builder)
    {
        builder.ToTable("Billings");

        builder.HasKey(x => x.Id);

        builder
            .Property(c => c.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Ignore(x => x.Events);

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnName("CreatedAt");

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.WalletId).IsRequired();

        builder.OwnsMany(x => x.FinancialItems, fb =>
        {
            fb.ToTable("FinancialItems");

            fb.HasKey(x => x.Id);

            fb.WithOwner()
                .HasForeignKey("BillingId");

            fb.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();


            fb.Property(x => x.CreatedAt)
                .IsRequired();

            fb.Property(c => c.Amount)
                .IsRequired()
                .HasConversion(money => money.Amount, value => new Money(value));

            fb.Property(c => c.RecordsAffected).IsRequired().HasDefaultValue(0);
            
            fb.OwnsMany(x => x.Commands, cfb =>
            {
                cfb.ToTable("Commands");
                cfb.Property<long>("Id").HasColumnType("bigint");
                cfb.HasKey("Id");
                cfb.WithOwner()
                    .HasForeignKey("FinancialItemId");

                cfb.Property(c => c.Code).IsRequired();
                cfb.Property(c => c.Title).IsRequired().HasMaxLength(20);
            });
        });
    }
}