﻿// <auto-generated />

#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Billing.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(BillingDbContext))]
    [Migration("20250118104412_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Billing.Domain.Billing.BillingAggregate", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<decimal>("Balance")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("CreatedAt");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<Guid>("WalletId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("Billings", (string)null);
                });

            modelBuilder.Entity("Billing.Domain.Billing.BillingAggregate", b =>
                {
                    b.OwnsMany("Billing.Domain.Billing.FinancialItem", "FinancialItems", b1 =>
                        {
                            b1.Property<long>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("bigint");

                            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b1.Property<long>("Id"));

                            b1.Property<decimal>("Amount")
                                .HasColumnType("decimal(18,2)");

                            b1.Property<long>("BillingId")
                                .HasColumnType("bigint");

                            b1.Property<DateTimeOffset>("CreatedAt")
                                .HasColumnType("datetimeoffset");

                            b1.Property<long>("RecordsAffected")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("bigint")
                                .HasDefaultValue(0L);

                            b1.HasKey("Id");

                            b1.HasIndex("BillingId");

                            b1.ToTable("FinancialItems", (string)null);

                            b1.WithOwner()
                                .HasForeignKey("BillingId");

                            b1.OwnsMany("Billing.Domain.Billing.CommandType", "Commands", b2 =>
                                {
                                    b2.Property<long>("Id")
                                        .ValueGeneratedOnAdd()
                                        .HasColumnType("bigint");

                                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b2.Property<long>("Id"));

                                    b2.Property<byte>("Code")
                                        .HasColumnType("tinyint");

                                    b2.Property<long>("FinancialItemId")
                                        .HasColumnType("bigint");

                                    b2.Property<string>("Title")
                                        .IsRequired()
                                        .HasMaxLength(20)
                                        .HasColumnType("nvarchar(20)");

                                    b2.HasKey("Id");

                                    b2.HasIndex("FinancialItemId");

                                    b2.ToTable("Commands", (string)null);

                                    b2.WithOwner()
                                        .HasForeignKey("FinancialItemId");
                                });

                            b1.Navigation("Commands");
                        });

                    b.Navigation("FinancialItems");
                });
#pragma warning restore 612, 618
        }
    }
}
