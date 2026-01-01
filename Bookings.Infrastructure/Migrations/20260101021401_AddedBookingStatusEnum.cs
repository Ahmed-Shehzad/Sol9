using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedBookingStatusEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.Sql(
                @"ALTER TABLE ""Bookings""
ALTER COLUMN ""Status"" TYPE integer
USING CASE ""Status""
    WHEN 'Created' THEN 0
    WHEN 'Confirmed' THEN 1
    WHEN '0' THEN 0
    WHEN '1' THEN 1
    ELSE 0
END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.Sql(
                @"ALTER TABLE ""Bookings""
ALTER COLUMN ""Status"" TYPE character varying(100)
USING CASE ""Status""
    WHEN 0 THEN 'Created'
    WHEN 1 THEN 'Confirmed'
    ELSE 'Created'
END;");
        }
    }
}
