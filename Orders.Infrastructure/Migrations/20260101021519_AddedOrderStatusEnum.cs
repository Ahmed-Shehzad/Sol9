using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedOrderStatusEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.Sql(
                @"ALTER TABLE ""Orders""
ALTER COLUMN ""Status"" TYPE integer
USING CASE ""Status""
    WHEN 'Created' THEN 0
    WHEN 'Booked' THEN 1
    WHEN 'Cancelled' THEN 2
    WHEN 'Expired' THEN 3
    WHEN 'Completed' THEN 4
    WHEN '0' THEN 0
    WHEN '1' THEN 1
    WHEN '2' THEN 2
    WHEN '3' THEN 3
    WHEN '4' THEN 4
    ELSE 0
END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.Sql(
                @"ALTER TABLE ""Orders""
ALTER COLUMN ""Status"" TYPE character varying(100)
USING CASE ""Status""
    WHEN 0 THEN 'Created'
    WHEN 1 THEN 'Booked'
    WHEN 2 THEN 'Cancelled'
    WHEN 3 THEN 'Expired'
    WHEN 4 THEN 'Completed'
    ELSE 'Created'
END;");
        }
    }
}
