using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Project420.Management.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersToManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PricelistItem_Pricelists_PricelistId",
                table: "PricelistItem");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermission_User_UserId",
                table: "UserPermission");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_User_UserId",
                table: "UserProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPermission",
                table: "UserPermission");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PricelistItem",
                table: "PricelistItem");

            migrationBuilder.DropIndex(
                name: "IX_PricelistItem_PricelistId",
                table: "PricelistItem");

            migrationBuilder.RenameTable(
                name: "UserPermission",
                newName: "UserPermissions");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "PricelistItem",
                newName: "PricelistItems");

            migrationBuilder.RenameIndex(
                name: "IX_UserPermission_UserId",
                table: "UserPermissions",
                newName: "IX_UserPermissions_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPermissions",
                table: "UserPermissions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PricelistItems",
                table: "PricelistItems",
                column: "Id");

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

            migrationBuilder.CreateIndex(
                name: "IX_PricelistItems_PricelistId_ProductId",
                table: "PricelistItems",
                columns: new[] { "PricelistId", "ProductId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PricelistItems_Pricelists_PricelistId",
                table: "PricelistItems",
                column: "PricelistId",
                principalTable: "Pricelists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Users_UserId",
                table: "UserPermissions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_Users_UserId",
                table: "UserProfiles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PricelistItems_Pricelists_PricelistId",
                table: "PricelistItems");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Users_UserId",
                table: "UserPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_Users_UserId",
                table: "UserProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsActive",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_LastLogin",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Role",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPermissions",
                table: "UserPermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserPermissions_ExpiresAt",
                table: "UserPermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserPermissions_IsActive",
                table: "UserPermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserPermissions_Permission",
                table: "UserPermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserPermissions_UserId_Permission",
                table: "UserPermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PricelistItems",
                table: "PricelistItems");

            migrationBuilder.DropIndex(
                name: "IX_PricelistItems_PricelistId_ProductId",
                table: "PricelistItems");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.RenameTable(
                name: "UserPermissions",
                newName: "UserPermission");

            migrationBuilder.RenameTable(
                name: "PricelistItems",
                newName: "PricelistItem");

            migrationBuilder.RenameIndex(
                name: "IX_UserPermissions_UserId",
                table: "UserPermission",
                newName: "IX_UserPermission_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPermission",
                table: "UserPermission",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PricelistItem",
                table: "PricelistItem",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PricelistItem_PricelistId",
                table: "PricelistItem",
                column: "PricelistId");

            migrationBuilder.AddForeignKey(
                name: "FK_PricelistItem_Pricelists_PricelistId",
                table: "PricelistItem",
                column: "PricelistId",
                principalTable: "Pricelists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermission_User_UserId",
                table: "UserPermission",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_User_UserId",
                table: "UserProfiles",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
