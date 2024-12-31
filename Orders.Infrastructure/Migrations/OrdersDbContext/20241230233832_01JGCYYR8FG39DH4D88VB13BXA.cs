using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Orders.Infrastructure.Migrations.OrdersDbContext
{
    /// <inheritdoc />
    public partial class _01JGCYYR8FG39DH4D88VB13BXA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "orders");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "order",
                schema: "orders",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    user_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    created_date_utc_at = table.Column<DateOnly>(type: "date", nullable: true),
                    created_time_utc_at = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_date_utc_at = table.Column<DateOnly>(type: "date", nullable: true),
                    updated_time_utc_at = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_date_utc_at = table.Column<DateOnly>(type: "date", nullable: true),
                    deleted_time_utc_at = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false, defaultValueSql: "gen_random_bytes(8)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox",
                schema: "orders",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    processed = table.Column<bool>(type: "boolean", nullable: false),
                    created_date_utc_at = table.Column<DateOnly>(type: "date", nullable: true),
                    created_time_utc_at = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_date_utc_at = table.Column<DateOnly>(type: "date", nullable: true),
                    updated_time_utc_at = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_date_utc_at = table.Column<DateOnly>(type: "date", nullable: true),
                    deleted_time_utc_at = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false, defaultValueSql: "gen_random_bytes(8)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "depot",
                schema: "orders",
                columns: table => new
                {
                    depot_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    order_id = table.Column<string>(type: "character varying(26)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_depot", x => new { x.order_id, x.depot_id });
                    table.ForeignKey(
                        name: "fk_depot_order_order_id",
                        column: x => x.order_id,
                        principalSchema: "orders",
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_billing_address",
                schema: "orders",
                columns: table => new
                {
                    order_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    point = table.Column<Point>(type: "geography", nullable: true),
                    street = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    number = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    zip_code = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    city = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    state = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    country = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_billing_address", x => x.order_id);
                    table.ForeignKey(
                        name: "fk_order_billing_address_order_order_id",
                        column: x => x.order_id,
                        principalSchema: "orders",
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_document",
                schema: "orders",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    order_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    meta_data = table.Column<JsonElement>(type: "jsonb", nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    user_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    created_date_utc_at = table.Column<DateOnly>(type: "date", nullable: true),
                    created_time_utc_at = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_date_utc_at = table.Column<DateOnly>(type: "date", nullable: true),
                    updated_time_utc_at = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_date_utc_at = table.Column<DateOnly>(type: "date", nullable: true),
                    deleted_time_utc_at = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false, defaultValueSql: "gen_random_bytes(8)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_document", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_document_order_order_id",
                        column: x => x.order_id,
                        principalSchema: "orders",
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_item",
                schema: "orders",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(26)", nullable: false),
                    order_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    product_id = table.Column<string>(type: "character varying(26)", nullable: true),
                    stop_item_id = table.Column<string>(type: "character varying(26)", nullable: true),
                    trip_id = table.Column<string>(type: "character varying(26)", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    metadata = table.Column<JsonElement>(type: "jsonb", nullable: true),
                    quantity_unit = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    quantity_value = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    weight_unit = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    weight_value = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    created_date_utc_at = table.Column<DateOnly>(type: "date", nullable: true),
                    created_time_utc_at = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_date_utc_at = table.Column<DateOnly>(type: "date", nullable: true),
                    updated_time_utc_at = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_date_utc_at = table.Column<DateOnly>(type: "date", nullable: true),
                    deleted_time_utc_at = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false, defaultValueSql: "gen_random_bytes(8)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_item_order_order_id",
                        column: x => x.order_id,
                        principalSchema: "orders",
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_shipping_address",
                schema: "orders",
                columns: table => new
                {
                    order_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    point = table.Column<Point>(type: "geography", nullable: true),
                    street = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    number = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    zip_code = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    city = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    state = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    country = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_shipping_address", x => x.order_id);
                    table.ForeignKey(
                        name: "fk_order_shipping_address_order_order_id",
                        column: x => x.order_id,
                        principalSchema: "orders",
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_time_frame",
                schema: "orders",
                columns: table => new
                {
                    order_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    from = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    to = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_time_frame", x => new { x.order_id, x.id });
                    table.ForeignKey(
                        name: "fk_order_time_frame_order_order_id",
                        column: x => x.order_id,
                        principalSchema: "orders",
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_transport_address",
                schema: "orders",
                columns: table => new
                {
                    order_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    point = table.Column<Point>(type: "geography", nullable: true),
                    street = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    number = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    zip_code = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    city = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    state = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    country = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_transport_address", x => x.order_id);
                    table.ForeignKey(
                        name: "fk_order_transport_address_order_order_id",
                        column: x => x.order_id,
                        principalSchema: "orders",
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_depot_depot_id",
                schema: "orders",
                table: "depot",
                column: "depot_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_tenant_id",
                schema: "orders",
                table: "order",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_type",
                schema: "orders",
                table: "order",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_order_user_id",
                schema: "orders",
                table: "order",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_document_order_id",
                schema: "orders",
                table: "order_document",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_document_tenant_id",
                schema: "orders",
                table: "order_document",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_document_user_id",
                schema: "orders",
                table: "order_document",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_order_id",
                schema: "orders",
                table: "order_item",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_product_id",
                schema: "orders",
                table: "order_item",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_stop_item_id",
                schema: "orders",
                table: "order_item",
                column: "stop_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_trip_id",
                schema: "orders",
                table: "order_item",
                column: "trip_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "depot",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "order_billing_address",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "order_document",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "order_item",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "order_shipping_address",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "order_time_frame",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "order_transport_address",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "outbox",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "order",
                schema: "orders");
        }
    }
}
