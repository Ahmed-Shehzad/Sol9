using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orders.Infrastructure.Migrations.OrdersDbContext
{
    /// <inheritdoc />
    public partial class complex_properties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_document_info",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "order_item_quantity",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "order_item_weight",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "order_item_info",
                schema: "orders");

            migrationBuilder.AddColumn<string>(
                name: "OrderItemInfo_Description",
                schema: "orders",
                table: "order_item",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<JsonElement>(
                name: "OrderItemInfo_MetaData",
                schema: "orders",
                table: "order_item",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderItemInfo_Quantity_Unit",
                schema: "orders",
                table: "order_item",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "OrderItemInfo_Quantity_Value",
                schema: "orders",
                table: "order_item",
                type: "numeric(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "OrderItemInfo_Weight_Unit",
                schema: "orders",
                table: "order_item",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "OrderItemInfo_Weight_Value",
                schema: "orders",
                table: "order_item",
                type: "numeric(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "orders",
                table: "order_document",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                schema: "orders",
                table: "order_document",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                schema: "orders",
                table: "order_document",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderItemInfo_Description",
                schema: "orders",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "OrderItemInfo_MetaData",
                schema: "orders",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "OrderItemInfo_Quantity_Unit",
                schema: "orders",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "OrderItemInfo_Quantity_Value",
                schema: "orders",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "OrderItemInfo_Weight_Unit",
                schema: "orders",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "OrderItemInfo_Weight_Value",
                schema: "orders",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "orders",
                table: "order_document");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "orders",
                table: "order_document");

            migrationBuilder.DropColumn(
                name: "Url",
                schema: "orders",
                table: "order_document");

            migrationBuilder.CreateTable(
                name: "order_document_info",
                schema: "orders",
                columns: table => new
                {
                    order_document_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_document_info", x => x.order_document_id);
                    table.ForeignKey(
                        name: "fk_order_document_info_order_document_order_document_id",
                        column: x => x.order_document_id,
                        principalSchema: "orders",
                        principalTable: "order_document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_item_info",
                schema: "orders",
                columns: table => new
                {
                    order_item_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    meta_data = table.Column<JsonElement>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_item_info", x => x.order_item_id);
                    table.ForeignKey(
                        name: "fk_order_item_info_order_item_order_item_id",
                        column: x => x.order_item_id,
                        principalSchema: "orders",
                        principalTable: "order_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_item_quantity",
                schema: "orders",
                columns: table => new
                {
                    order_item_info_order_item_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    unit = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    value = table.Column<decimal>(type: "numeric(18,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_item_quantity", x => x.order_item_info_order_item_id);
                    table.ForeignKey(
                        name: "fk_order_item_quantity_order_item_info_order_item_info_order_item_id",
                        column: x => x.order_item_info_order_item_id,
                        principalSchema: "orders",
                        principalTable: "order_item_info",
                        principalColumn: "order_item_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_item_weight",
                schema: "orders",
                columns: table => new
                {
                    order_item_info_order_item_id = table.Column<string>(type: "character varying(26)", nullable: false),
                    unit = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    value = table.Column<decimal>(type: "numeric(18,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_item_weight", x => x.order_item_info_order_item_id);
                    table.ForeignKey(
                        name: "fk_order_item_weight_order_item_info_order_item_info_order_item_id",
                        column: x => x.order_item_info_order_item_id,
                        principalSchema: "orders",
                        principalTable: "order_item_info",
                        principalColumn: "order_item_id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
