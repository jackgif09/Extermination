using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Extermination.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PestType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PreferredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ServiceRequests",
                columns: new[] { "Id", "Address", "CreatedAt", "CustomerName", "Description", "Email", "PestType", "Phone", "PreferredDate", "Status" },
                values: new object[,]
                {
                    { 1, "123 Maple Street, Springfield, IL 62701", new DateTime(2024, 6, 1, 8, 0, 0, 0, DateTimeKind.Utc), "Alice Johnson", "Ant trail coming in through kitchen window.", "alice@example.com", 0, "555-123-4567", new DateTime(2024, 6, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 2, "456 Oak Avenue, Shelbyville, IL 62565", new DateTime(2024, 6, 28, 10, 30, 0, 0, DateTimeKind.Utc), "Bob Martinez", "Hearing noises in the attic at night.", "bob@example.com", 5, "555-987-6543", new DateTime(2024, 7, 10, 0, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 3, "789 Pine Road, Capital City, IL 62702", new DateTime(2024, 7, 1, 14, 0, 0, 0, DateTimeKind.Utc), "Carol White", "Noticed wood damage in basement beams.", "carol@example.com", 7, "555-246-8013", null, 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceRequests");
        }
    }
}
