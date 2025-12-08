using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Project420.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddStationConnections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StationConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HostName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DatabaseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EncryptedConnectionString = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsAuthorized = table.Column<bool>(type: "bit", nullable: false),
                    AccessExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationConnections", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "StationConnections",
                columns: new[] { "Id", "AccessExpiresAt", "CompanyName", "CreatedAt", "CreatedBy", "DatabaseName", "DeletedAt", "DeletedBy", "EncryptedConnectionString", "HostName", "IsAuthorized", "IsDeleted", "ModifiedAt", "ModifiedBy", "Notes" },
                values: new object[,]
                {
                    { 1, null, "Project420 - Main Store", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", "Project420_Dev", null, null, "Server=JASON\\SQLDEVED;Database=Project420_Dev;User Id=sa;Password=ZAQxsw123;TrustServerCertificate=True;", "POS-STORE-01", true, false, null, null, "Main POS terminal for Store 01" },
                    { 2, null, "Project420 - Branch Store", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", "Project420_Dev2", null, null, "Server=JASON\\SQLDEVED;Database=Project420_Dev2;User Id=sa;Password=ZAQxsw123;TrustServerCertificate=True;", "POS-STORE-01", true, false, null, null, "Backup access to Branch Store for emergencies" },
                    { 3, new DateTime(2025, 12, 31, 23, 59, 59, 0, DateTimeKind.Utc), "Project420 - Main Store", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", "Project420_Dev", null, null, "Server=JASON\\SQLDEVED;Database=Project420_Dev;User Id=sa;Password=ZAQxsw123;TrustServerCertificate=True;", "MOBILE-REP-01", true, false, null, null, "Regional manager tablet - temporary access for Q4 2025" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_StationConnections_AccessExpiresAt",
                table: "StationConnections",
                column: "AccessExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_StationConnections_DatabaseName",
                table: "StationConnections",
                column: "DatabaseName");

            migrationBuilder.CreateIndex(
                name: "IX_StationConnections_HostName",
                table: "StationConnections",
                column: "HostName");

            migrationBuilder.CreateIndex(
                name: "IX_StationConnections_HostName_IsAuthorized",
                table: "StationConnections",
                columns: new[] { "HostName", "IsAuthorized" });

            migrationBuilder.CreateIndex(
                name: "IX_StationConnections_IsAuthorized",
                table: "StationConnections",
                column: "IsAuthorized");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StationConnections");
        }
    }
}
