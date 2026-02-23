using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Extermination.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledForAndPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ServiceRequests",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledFor",
                table: "ServiceRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ServiceRequests",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Price", "ScheduledFor" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ServiceRequests",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Price", "ScheduledFor" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ServiceRequests",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Price", "ScheduledFor" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ScheduledFor",
                table: "ServiceRequests");
        }
    }
}
