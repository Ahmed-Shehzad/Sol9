using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using Bookings.Infrastructure.Contexts;

namespace Bookings.Infrastructure.Migrations;

[DbContext(typeof(BookingsDbContext))]
public partial class BookingsDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.1");
        modelBuilder.HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.Entity("Bookings.Domain.Entities.Booking", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<DateTime>("CreatedAtUtc")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("CustomerName")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)");

            b.Property<DateTime?>("DeletedAtUtc")
                .HasColumnType("timestamp with time zone");

            b.Property<bool>("IsDeleted")
                .HasColumnType("boolean");

            b.Property<Guid>("OrderId")
                .HasColumnType("uuid");

            b.Property<string>("Status")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)");

            b.Property<DateTime>("UpdatedAtUtc")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.ToTable("Bookings");
        });
    }
}
