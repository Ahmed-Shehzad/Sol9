using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orders.Infrastructure.Migrations.OrdersDbContext
{
    /// <inheritdoc />
    public partial class rename_columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Url",
                schema: "orders",
                table: "order_document",
                newName: "DocumentInfo_Url");

            migrationBuilder.RenameColumn(
                name: "Type",
                schema: "orders",
                table: "order_document",
                newName: "DocumentInfo_Type");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "orders",
                table: "order_document",
                newName: "DocumentInfo_Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DocumentInfo_Url",
                schema: "orders",
                table: "order_document",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "DocumentInfo_Type",
                schema: "orders",
                table: "order_document",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "DocumentInfo_Name",
                schema: "orders",
                table: "order_document",
                newName: "Name");
        }
    }
}
