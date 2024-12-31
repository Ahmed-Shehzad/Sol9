﻿// <auto-generated />
using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Orders.Infrastructure.Contexts;

#nullable disable

namespace Orders.Infrastructure.Migrations.OrdersDbContext
{
    [DbContext(typeof(Contexts.OrdersDbContext))]
    [Migration("20241230233832_01JGCYYR8FG39DH4D88VB13BXA")]
    partial class _01JGCYYR8FG39DH4D88VB13BXA
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("orders")
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "pgcrypto");
            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "postgis");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BuildingBlocks.Domain.Aggregates.Entities.Outbox", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("character varying(26)")
                        .HasColumnName("id");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text")
                        .HasColumnName("created_by");

                    b.Property<DateOnly?>("CreatedDateUtcAt")
                        .HasColumnType("date")
                        .HasColumnName("created_date_utc_at");

                    b.Property<TimeOnly?>("CreatedTimeUtcAt")
                        .HasColumnType("time without time zone")
                        .HasColumnName("created_time_utc_at");

                    b.Property<string>("DeletedBy")
                        .HasColumnType("text")
                        .HasColumnName("deleted_by");

                    b.Property<DateOnly?>("DeletedDateUtcAt")
                        .HasColumnType("date")
                        .HasColumnName("deleted_date_utc_at");

                    b.Property<TimeOnly?>("DeletedTimeUtcAt")
                        .HasColumnType("time without time zone")
                        .HasColumnName("deleted_time_utc_at");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("payload");

                    b.Property<bool>("Processed")
                        .HasColumnType("boolean")
                        .HasColumnName("processed");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("bytea")
                        .HasColumnName("row_version")
                        .HasDefaultValueSql("gen_random_bytes(8)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("type");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text")
                        .HasColumnName("updated_by");

                    b.Property<DateOnly?>("UpdatedDateUtcAt")
                        .HasColumnType("date")
                        .HasColumnName("updated_date_utc_at");

                    b.Property<TimeOnly?>("UpdatedTimeUtcAt")
                        .HasColumnType("time without time zone")
                        .HasColumnName("updated_time_utc_at");

                    b.HasKey("Id")
                        .HasName("pk_outbox");

                    b.ToTable("outbox", "orders");
                });

            modelBuilder.Entity("Orders.Domain.Aggregates.Entities.OrderDocument", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("character varying(26)")
                        .HasColumnName("id");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text")
                        .HasColumnName("created_by");

                    b.Property<DateOnly?>("CreatedDateUtcAt")
                        .HasColumnType("date")
                        .HasColumnName("created_date_utc_at");

                    b.Property<TimeOnly?>("CreatedTimeUtcAt")
                        .HasColumnType("time without time zone")
                        .HasColumnName("created_time_utc_at");

                    b.Property<string>("DeletedBy")
                        .HasColumnType("text")
                        .HasColumnName("deleted_by");

                    b.Property<DateOnly?>("DeletedDateUtcAt")
                        .HasColumnType("date")
                        .HasColumnName("deleted_date_utc_at");

                    b.Property<TimeOnly?>("DeletedTimeUtcAt")
                        .HasColumnType("time without time zone")
                        .HasColumnName("deleted_time_utc_at");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.Property<JsonElement?>("MetaData")
                        .HasColumnType("jsonb")
                        .HasColumnName("meta_data");

                    b.Property<string>("OrderId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasColumnName("order_id");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("bytea")
                        .HasColumnName("row_version")
                        .HasDefaultValueSql("gen_random_bytes(8)");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasColumnName("tenant_id");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text")
                        .HasColumnName("updated_by");

                    b.Property<DateOnly?>("UpdatedDateUtcAt")
                        .HasColumnType("date")
                        .HasColumnName("updated_date_utc_at");

                    b.Property<TimeOnly?>("UpdatedTimeUtcAt")
                        .HasColumnType("time without time zone")
                        .HasColumnName("updated_time_utc_at");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasColumnName("user_id");

                    b.ComplexProperty<Dictionary<string, object>>("DocumentInfo", "Orders.Domain.Aggregates.Entities.OrderDocument.DocumentInfo#OrderDocumentInfo", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("name");

                            b1.Property<string>("Type")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("type");

                            b1.Property<string>("Url")
                                .IsRequired()
                                .HasColumnType("text")
                                .HasColumnName("url");
                        });

                    b.HasKey("Id")
                        .HasName("pk_order_document");

                    b.HasIndex("OrderId");

                    b.HasIndex("TenantId");

                    b.HasIndex("UserId");

                    b.ToTable("order_document", "orders");
                });

            modelBuilder.Entity("Orders.Domain.Aggregates.Entities.OrderItem", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("character varying(26)")
                        .HasColumnName("id");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text")
                        .HasColumnName("created_by");

                    b.Property<DateOnly?>("CreatedDateUtcAt")
                        .HasColumnType("date")
                        .HasColumnName("created_date_utc_at");

                    b.Property<TimeOnly?>("CreatedTimeUtcAt")
                        .HasColumnType("time without time zone")
                        .HasColumnName("created_time_utc_at");

                    b.Property<string>("DeletedBy")
                        .HasColumnType("text")
                        .HasColumnName("deleted_by");

                    b.Property<DateOnly?>("DeletedDateUtcAt")
                        .HasColumnType("date")
                        .HasColumnName("deleted_date_utc_at");

                    b.Property<TimeOnly?>("DeletedTimeUtcAt")
                        .HasColumnType("time without time zone")
                        .HasColumnName("deleted_time_utc_at");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.Property<string>("OrderId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasColumnName("order_id");

                    b.Property<string>("ProductId")
                        .HasColumnType("character varying(26)")
                        .HasColumnName("product_id");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("bytea")
                        .HasColumnName("row_version")
                        .HasDefaultValueSql("gen_random_bytes(8)");

                    b.Property<string>("StopItemId")
                        .HasColumnType("character varying(26)")
                        .HasColumnName("stop_item_id");

                    b.Property<string>("TripId")
                        .HasColumnType("character varying(26)")
                        .HasColumnName("trip_id");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text")
                        .HasColumnName("updated_by");

                    b.Property<DateOnly?>("UpdatedDateUtcAt")
                        .HasColumnType("date")
                        .HasColumnName("updated_date_utc_at");

                    b.Property<TimeOnly?>("UpdatedTimeUtcAt")
                        .HasColumnType("time without time zone")
                        .HasColumnName("updated_time_utc_at");

                    b.ComplexProperty<Dictionary<string, object>>("OrderItemInfo", "Orders.Domain.Aggregates.Entities.OrderItem.OrderItemInfo#OrderItemInfo", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<string>("Description")
                                .HasColumnType("text")
                                .HasColumnName("description");

                            b1.Property<JsonElement?>("MetaData")
                                .HasColumnType("jsonb")
                                .HasColumnName("metadata");

                            b1.ComplexProperty<Dictionary<string, object>>("Quantity", "Orders.Domain.Aggregates.Entities.OrderItem.OrderItemInfo#OrderItemInfo.Quantity#UnitValue<decimal>", b2 =>
                                {
                                    b2.IsRequired();

                                    b2.Property<string>("Unit")
                                        .IsRequired()
                                        .HasMaxLength(255)
                                        .HasColumnType("character varying(255)")
                                        .HasColumnName("quantity_unit");

                                    b2.Property<decimal>("Value")
                                        .HasColumnType("decimal(18, 3)")
                                        .HasColumnName("quantity_value");
                                });

                            b1.ComplexProperty<Dictionary<string, object>>("Weight", "Orders.Domain.Aggregates.Entities.OrderItem.OrderItemInfo#OrderItemInfo.Weight#UnitValue<decimal>", b2 =>
                                {
                                    b2.IsRequired();

                                    b2.Property<string>("Unit")
                                        .IsRequired()
                                        .HasMaxLength(255)
                                        .HasColumnType("character varying(255)")
                                        .HasColumnName("weight_unit");

                                    b2.Property<decimal>("Value")
                                        .HasColumnType("decimal(18, 3)")
                                        .HasColumnName("weight_value");
                                });
                        });

                    b.HasKey("Id")
                        .HasName("pk_order_item");

                    b.HasIndex("OrderId");

                    b.HasIndex("ProductId");

                    b.HasIndex("StopItemId");

                    b.HasIndex("TripId");

                    b.ToTable("order_item", "orders");
                });

            modelBuilder.Entity("Orders.Domain.Aggregates.Order", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("character varying(26)")
                        .HasColumnName("id");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text")
                        .HasColumnName("created_by");

                    b.Property<DateOnly?>("CreatedDateUtcAt")
                        .HasColumnType("date")
                        .HasColumnName("created_date_utc_at");

                    b.Property<TimeOnly?>("CreatedTimeUtcAt")
                        .HasColumnType("time without time zone")
                        .HasColumnName("created_time_utc_at");

                    b.Property<string>("DeletedBy")
                        .HasColumnType("text")
                        .HasColumnName("deleted_by");

                    b.Property<DateOnly?>("DeletedDateUtcAt")
                        .HasColumnType("date")
                        .HasColumnName("deleted_date_utc_at");

                    b.Property<TimeOnly?>("DeletedTimeUtcAt")
                        .HasColumnType("time without time zone")
                        .HasColumnName("deleted_time_utc_at");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("bytea")
                        .HasColumnName("row_version")
                        .HasDefaultValueSql("gen_random_bytes(8)");

                    b.Property<long>("Status")
                        .HasColumnType("bigint")
                        .HasColumnName("status");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasColumnName("tenant_id");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("type");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text")
                        .HasColumnName("updated_by");

                    b.Property<DateOnly?>("UpdatedDateUtcAt")
                        .HasColumnType("date")
                        .HasColumnName("updated_date_utc_at");

                    b.Property<TimeOnly?>("UpdatedTimeUtcAt")
                        .HasColumnType("time without time zone")
                        .HasColumnName("updated_time_utc_at");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_order");

                    b.HasIndex("TenantId");

                    b.HasIndex("Type");

                    b.HasIndex("UserId");

                    b.ToTable("order", "orders");
                });

            modelBuilder.Entity("Orders.Domain.Aggregates.Entities.OrderDocument", b =>
                {
                    b.HasOne("Orders.Domain.Aggregates.Order", "Order")
                        .WithMany("Documents")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_order_document_order_order_id");

                    b.Navigation("Order");
                });

            modelBuilder.Entity("Orders.Domain.Aggregates.Entities.OrderItem", b =>
                {
                    b.HasOne("Orders.Domain.Aggregates.Order", "Order")
                        .WithMany("Items")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_order_item_order_order_id");

                    b.Navigation("Order");
                });

            modelBuilder.Entity("Orders.Domain.Aggregates.Order", b =>
                {
                    b.OwnsMany("Orders.Domain.Aggregates.Entities.ValueObjects.Depot", "Depots", b1 =>
                        {
                            b1.Property<string>("OrderId")
                                .HasColumnType("character varying(26)")
                                .HasColumnName("order_id");

                            b1.Property<string>("DepotId")
                                .HasColumnType("character varying(26)")
                                .HasColumnName("depot_id");

                            b1.HasKey("OrderId", "DepotId")
                                .HasName("pk_depot");

                            b1.HasIndex("DepotId");

                            b1.ToTable("depot", "orders");

                            b1.WithOwner()
                                .HasForeignKey("OrderId")
                                .HasConstraintName("fk_depot_order_order_id");
                        });

                    b.OwnsMany("Orders.Domain.Aggregates.Entities.ValueObjects.OrderTimeFrame", "TimeFrames", b1 =>
                        {
                            b1.Property<string>("OrderId")
                                .HasColumnType("character varying(26)")
                                .HasColumnName("order_id");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer")
                                .HasColumnName("id");

                            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b1.Property<int>("Id"));

                            b1.Property<int>("DayOfWeek")
                                .HasColumnType("integer")
                                .HasColumnName("day_of_week");

                            b1.Property<TimeOnly>("From")
                                .HasColumnType("time without time zone")
                                .HasColumnName("from");

                            b1.Property<TimeOnly>("To")
                                .HasColumnType("time without time zone")
                                .HasColumnName("to");

                            b1.HasKey("OrderId", "Id")
                                .HasName("pk_order_time_frame");

                            b1.ToTable("order_time_frame", "orders");

                            b1.WithOwner()
                                .HasForeignKey("OrderId")
                                .HasConstraintName("fk_order_time_frame_order_order_id");
                        });

                    b.OwnsOne("BuildingBlocks.Domain.Aggregates.Entities.ValueObjects.Address", "BillingAddress", b1 =>
                        {
                            b1.Property<string>("OrderId")
                                .HasColumnType("character varying(26)")
                                .HasColumnName("order_id");

                            b1.Property<string>("City")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("city");

                            b1.Property<string>("Country")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("country");

                            b1.Property<string>("Number")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("number");

                            b1.Property<Point>("Point")
                                .HasColumnType("geography")
                                .HasColumnName("point");

                            b1.Property<string>("State")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("state");

                            b1.Property<string>("Street")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("street");

                            b1.Property<string>("ZipCode")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("zip_code");

                            b1.HasKey("OrderId")
                                .HasName("pk_order_billing_address");

                            b1.ToTable("order_billing_address", "orders");

                            b1.WithOwner()
                                .HasForeignKey("OrderId")
                                .HasConstraintName("fk_order_billing_address_order_order_id");
                        });

                    b.OwnsOne("BuildingBlocks.Domain.Aggregates.Entities.ValueObjects.Address", "ShippingAddress", b1 =>
                        {
                            b1.Property<string>("OrderId")
                                .HasColumnType("character varying(26)")
                                .HasColumnName("order_id");

                            b1.Property<string>("City")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("city");

                            b1.Property<string>("Country")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("country");

                            b1.Property<string>("Number")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("number");

                            b1.Property<Point>("Point")
                                .HasColumnType("geography")
                                .HasColumnName("point");

                            b1.Property<string>("State")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("state");

                            b1.Property<string>("Street")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("street");

                            b1.Property<string>("ZipCode")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("zip_code");

                            b1.HasKey("OrderId")
                                .HasName("pk_order_shipping_address");

                            b1.ToTable("order_shipping_address", "orders");

                            b1.WithOwner()
                                .HasForeignKey("OrderId")
                                .HasConstraintName("fk_order_shipping_address_order_order_id");
                        });

                    b.OwnsOne("BuildingBlocks.Domain.Aggregates.Entities.ValueObjects.Address", "TransportAddress", b1 =>
                        {
                            b1.Property<string>("OrderId")
                                .HasColumnType("character varying(26)")
                                .HasColumnName("order_id");

                            b1.Property<string>("City")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("city");

                            b1.Property<string>("Country")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("country");

                            b1.Property<string>("Number")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("number");

                            b1.Property<Point>("Point")
                                .HasColumnType("geography")
                                .HasColumnName("point");

                            b1.Property<string>("State")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("state");

                            b1.Property<string>("Street")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("street");

                            b1.Property<string>("ZipCode")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("zip_code");

                            b1.HasKey("OrderId")
                                .HasName("pk_order_transport_address");

                            b1.ToTable("order_transport_address", "orders");

                            b1.WithOwner()
                                .HasForeignKey("OrderId")
                                .HasConstraintName("fk_order_transport_address_order_order_id");
                        });

                    b.Navigation("BillingAddress");

                    b.Navigation("Depots");

                    b.Navigation("ShippingAddress");

                    b.Navigation("TimeFrames");

                    b.Navigation("TransportAddress");
                });

            modelBuilder.Entity("Orders.Domain.Aggregates.Order", b =>
                {
                    b.Navigation("Documents");

                    b.Navigation("Items");
                });
#pragma warning restore 612, 618
        }
    }
}
