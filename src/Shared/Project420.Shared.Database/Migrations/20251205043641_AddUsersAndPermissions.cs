using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Project420.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PasswordSalt = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    AccountLockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginIpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    PasswordLastChangedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PasswordResetExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorSecret = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Permission = table.Column<int>(type: "int", nullable: false),
                    IsGranted = table.Column<bool>(type: "bit", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_UserPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccountLockedAt", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Email", "FailedLoginAttempts", "FirstName", "IsActive", "IsDeleted", "IsLocked", "LastLoginAt", "LastLoginIpAddress", "LastName", "ModifiedAt", "ModifiedBy", "PasswordExpiresAt", "PasswordHash", "PasswordLastChangedAt", "PasswordResetExpiresAt", "PasswordResetToken", "PasswordSalt", "Role", "TwoFactorEnabled", "TwoFactorSecret", "Username" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, "superadmin@project420.local", 0, "Super", true, false, false, null, null, "Admin", null, null, null, "$2a$11$EK5kC8qGqJH5rZvK.YjJGuH7Z6uYF6NjN9XoYdQdH3xRZLqKxhE/G", null, null, null, null, "SuperAdmin", false, null, "superadmin" },
                    { 2, null, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, "admin@project420.local", 0, "System", true, false, false, null, null, "Administrator", null, null, null, "$2a$11$EK5kC8qGqJH5rZvK.YjJGuH7Z6uYF6NjN9XoYdQdH3xRZLqKxhE/G", null, null, null, null, "Admin", false, null, "admin" },
                    { 3, null, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, "manager@project420.local", 0, "Store", true, false, false, null, null, "Manager", null, null, null, "$2a$11$EK5kC8qGqJH5rZvK.YjJGuH7Z6uYF6NjN9XoYdQdH3xRZLqKxhE/G", null, null, null, null, "Manager", false, null, "manager" },
                    { 4, null, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, "cashier@project420.local", 0, "POS", true, false, false, null, null, "Cashier", null, null, null, "$2a$11$EK5kC8qGqJH5rZvK.YjJGuH7Z6uYF6NjN9XoYdQdH3xRZLqKxhE/G", null, null, null, null, "Cashier", false, null, "cashier" },
                    { 5, null, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, "inventory@project420.local", 0, "Inventory", true, false, false, null, null, "Manager", null, null, null, "$2a$11$EK5kC8qGqJH5rZvK.YjJGuH7Z6uYF6NjN9XoYdQdH3xRZLqKxhE/G", null, null, null, null, "Inventory", false, null, "inventory" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_ExpiresAt",
                table: "UserPermissions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_IsActive",
                table: "UserPermissions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_Permission",
                table: "UserPermissions",
                column: "Permission");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_UserId",
                table: "UserPermissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_UserId_Permission",
                table: "UserPermissions",
                columns: new[] { "UserId", "Permission" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastLogin",
                table: "Users",
                column: "LastLoginAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
