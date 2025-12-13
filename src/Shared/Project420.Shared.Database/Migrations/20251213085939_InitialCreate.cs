using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Project420.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                });

            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    ErrorLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ErrorType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StackTrace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InnerException = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    RequestPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HttpMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogs", x => x.ErrorLogId);
                });

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

            migrationBuilder.CreateTable(
                name: "TransactionNumberSequences",
                columns: table => new
                {
                    SequenceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CurrentSequence = table.Column<long>(type: "bigint", nullable: false),
                    StartingSequence = table.Column<long>(type: "bigint", nullable: false),
                    PaddingLength = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastGeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionNumberSequences", x => x.SequenceId);
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

            migrationBuilder.InsertData(
                table: "TransactionNumberSequences",
                columns: new[] { "SequenceId", "CreatedAt", "CreatedBy", "CurrentSequence", "Description", "IsActive", "LastGeneratedAt", "LastUpdatedAt", "LastUpdatedBy", "PaddingLength", "Prefix", "StartingSequence", "TransactionType" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Sales invoices - formal account invoices", true, null, null, null, 5, "INV", 1L, 4 },
                    { 2, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Credit notes - refunds and returns", true, null, null, null, 5, "CRN", 1L, 5 },
                    { 3, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Point of Sale transactions - retail sales", true, null, null, null, 5, "", 1L, 1 },
                    { 4, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Goods received notes - stock received from suppliers", true, null, null, null, 5, "GRV", 1L, 2 },
                    { 5, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Returns to supplier - defective/unwanted stock", true, null, null, null, 5, "RTS", 1L, 3 },
                    { 6, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Stock adjustments - internal corrections", true, null, null, null, 5, "ADJ", 1L, 6 },
                    { 7, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Customer payments - cash/card/EFT received", true, null, null, null, 5, "PAY", 1L, 7 },
                    { 8, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Sales quotations - non-binding estimates", true, null, null, null, 5, "QTE", 1L, 8 },
                    { 9, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Layby transactions - deposit for future collection", true, null, null, null, 5, "LAY", 1L, 9 },
                    { 10, new DateTime(2025, 12, 6, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", 0L, "Stock transfers - inter-location movements", true, null, null, null, 5, "TRF", 1L, 10 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActionType",
                table: "AuditLogs",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Entity",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Entity_Date",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Module",
                table: "AuditLogs",
                column: "Module");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OccurredAt",
                table: "AuditLogs",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Severity",
                table: "AuditLogs",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_IsResolved",
                table: "ErrorLogs",
                column: "IsResolved");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_OccurredAt",
                table: "ErrorLogs",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_Resolved_Severity_Date",
                table: "ErrorLogs",
                columns: new[] { "IsResolved", "Severity", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_Severity",
                table: "ErrorLogs",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_Source",
                table: "ErrorLogs",
                column: "Source");

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

            migrationBuilder.CreateIndex(
                name: "IX_TransactionNumberSequences_IsActive",
                table: "TransactionNumberSequences",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionNumberSequences_LastGeneratedAt",
                table: "TransactionNumberSequences",
                column: "LastGeneratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionNumberSequences_TransactionType_Unique",
                table: "TransactionNumberSequences",
                column: "TransactionType",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ErrorLogs");

            migrationBuilder.DropTable(
                name: "StationConnections");

            migrationBuilder.DropTable(
                name: "TransactionNumberSequences");
        }
    }
}
