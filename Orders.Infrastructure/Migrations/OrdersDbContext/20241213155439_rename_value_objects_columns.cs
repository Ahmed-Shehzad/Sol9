using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orders.Infrastructure.Migrations.OrdersDbContext
{
    /// <inheritdoc />
    public partial class rename_value_objects_columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderItemInfo_Weight_Value",
                schema: "orders",
                table: "order_item",
                newName: "weight_value");

            migrationBuilder.RenameColumn(
                name: "OrderItemInfo_Weight_Unit",
                schema: "orders",
                table: "order_item",
                newName: "weight_unit");

            migrationBuilder.RenameColumn(
                name: "OrderItemInfo_Quantity_Value",
                schema: "orders",
                table: "order_item",
                newName: "quantity_value");

            migrationBuilder.RenameColumn(
                name: "OrderItemInfo_Quantity_Unit",
                schema: "orders",
                table: "order_item",
                newName: "quantity_unit");

            migrationBuilder.RenameColumn(
                name: "OrderItemInfo_MetaData",
                schema: "orders",
                table: "order_item",
                newName: "metadata");

            migrationBuilder.RenameColumn(
                name: "OrderItemInfo_Description",
                schema: "orders",
                table: "order_item",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "DocumentInfo_Url",
                schema: "orders",
                table: "order_document",
                newName: "url");

            migrationBuilder.RenameColumn(
                name: "DocumentInfo_Type",
                schema: "orders",
                table: "order_document",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "DocumentInfo_Name",
                schema: "orders",
                table: "order_document",
                newName: "name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "weight_value",
                schema: "orders",
                table: "order_item",
                newName: "OrderItemInfo_Weight_Value");

            migrationBuilder.RenameColumn(
                name: "weight_unit",
                schema: "orders",
                table: "order_item",
                newName: "OrderItemInfo_Weight_Unit");

            migrationBuilder.RenameColumn(
                name: "quantity_value",
                schema: "orders",
                table: "order_item",
                newName: "OrderItemInfo_Quantity_Value");

            migrationBuilder.RenameColumn(
                name: "quantity_unit",
                schema: "orders",
                table: "order_item",
                newName: "OrderItemInfo_Quantity_Unit");

            migrationBuilder.RenameColumn(
                name: "metadata",
                schema: "orders",
                table: "order_item",
                newName: "OrderItemInfo_MetaData");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "orders",
                table: "order_item",
                newName: "OrderItemInfo_Description");

            migrationBuilder.RenameColumn(
                name: "url",
                schema: "orders",
                table: "order_document",
                newName: "DocumentInfo_Url");

            migrationBuilder.RenameColumn(
                name: "type",
                schema: "orders",
                table: "order_document",
                newName: "DocumentInfo_Type");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "orders",
                table: "order_document",
                newName: "DocumentInfo_Name");
        }
    }
}
