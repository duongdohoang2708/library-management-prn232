using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LibraryManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AccountMemberStaffRoleModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookReviews_Users_UserId",
                table: "BookReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_BorrowTransactions_Users_UserId",
                table: "BorrowTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_Users_UserId",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_UserId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_UserId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Users_UserId",
                table: "Roles");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Roles_UserId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Roles");

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GoogleId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsPasswordSet = table.Column<bool>(type: "bit", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordResetCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    PasswordResetCodeExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordResetCodeVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MemberCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.MemberId);
                    table.ForeignKey(
                        name: "FK_Members_Accounts_UserId",
                        column: x => x.UserId,
                        principalTable: "Accounts",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Staffs",
                columns: table => new
                {
                    StaffId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    StaffCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    HiredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staffs", x => x.StaffId);
                    table.ForeignKey(
                        name: "FK_Staffs_Accounts_UserId",
                        column: x => x.UserId,
                        principalTable: "Accounts",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Staffs_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "UserId", "Address", "CreatedAt", "DateOfBirth", "DeletedAt", "Email", "FullName", "GoogleId", "IsActive", "IsDeleted", "IsPasswordSet", "LastLoginAt", "PasswordHash", "PasswordResetCode", "PasswordResetCodeExpiresAt", "PasswordResetCodeVerifiedAt", "Phone", "RefreshToken", "RefreshTokenExpiry", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { 1, "Hanoi, Vietnam", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1995, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "admin@lms.com", "System Administrator", null, true, false, true, null, "AQAAAAIAAYagAAAAEEFkbWluU2FsdDEyMzQ1Njel4yPDX+DW0RDQGojuqKgDm9LCHqF6JL2jjZxcKy0h1A==", null, null, null, "0900000001", null, null, null, "admin" },
                    { 2, "Ho Chi Minh City", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1994, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "librarian1@lms.com", "Nguyen Van A", null, true, false, true, null, "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==", null, null, null, "0900000002", null, null, null, "librarian1" },
                    { 3, "Da Nang", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2000, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member1@mail.com", "Tran Thi B", null, true, false, true, null, "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==", null, null, null, "0900000003", null, null, null, "member1" },
                    { 4, "Hai Phong", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1999, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member2@mail.com", "Le Van C", null, true, false, true, null, "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==", null, null, null, "0900000004", null, null, null, "member2" },
                    { 5, "Can Tho", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2001, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member3@mail.com", "Pham Thi D", null, true, false, true, null, "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==", null, null, null, "0900000005", null, null, null, "member3" },
                    { 6, "Quang Ninh", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1998, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member4@mail.com", "Hoang Van E", null, true, false, true, null, "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==", null, null, null, "0900000006", null, null, null, "member4" },
                    { 7, "Hue", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2002, 2, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member5@mail.com", "Do Thi F", null, true, false, true, null, "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==", null, null, null, "0900000007", null, null, null, "member5" },
                    { 8, "Binh Duong", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1997, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member6@mail.com", "Vu Van G", null, true, false, true, null, "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==", null, null, null, "0900000008", null, null, null, "member6" },
                    { 9, "Dong Nai", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2003, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member7@mail.com", "Dang Thi H", null, true, false, true, null, "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==", null, null, null, "0900000009", null, null, null, "member7" },
                    { 10, "Nam Dinh", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1996, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member8@mail.com", "Bui Van I", null, true, false, true, null, "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==", null, null, null, "0900000010", null, null, null, "member8" }
                });

            migrationBuilder.InsertData(
                table: "Members",
                columns: new[] { "MemberId", "CreatedAt", "DeletedAt", "IsDeleted", "JoinedAt", "MemberCode", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MEM00001", null, 2 },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MEM00002", null, 3 },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MEM00003", null, 4 },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MEM00004", null, 5 },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MEM00005", null, 6 },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MEM00006", null, 7 },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MEM00007", null, 8 },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MEM00008", null, 10 }
                });

            migrationBuilder.InsertData(
                table: "Staffs",
                columns: new[] { "StaffId", "CreatedAt", "DeletedAt", "HiredAt", "IsDeleted", "RoleId", "StaffCode", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 1, "STF00001", null, 1 },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 2, "STF00002", null, 9 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Email",
                table: "Accounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_MemberCode",
                table: "Members",
                column: "MemberCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_UserId",
                table: "Members",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_RoleId",
                table: "Staffs",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_StaffCode",
                table: "Staffs",
                column: "StaffCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_UserId",
                table: "Staffs",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BookReviews_Accounts_UserId",
                table: "BookReviews",
                column: "UserId",
                principalTable: "Accounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowTransactions_Accounts_UserId",
                table: "BorrowTransactions",
                column: "UserId",
                principalTable: "Accounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_Accounts_UserId",
                table: "Notification",
                column: "UserId",
                principalTable: "Accounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Accounts_UserId",
                table: "Payments",
                column: "UserId",
                principalTable: "Accounts",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Accounts_UserId",
                table: "Reservations",
                column: "UserId",
                principalTable: "Accounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookReviews_Accounts_UserId",
                table: "BookReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_BorrowTransactions_Accounts_UserId",
                table: "BorrowTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_Accounts_UserId",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Accounts_UserId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Accounts_UserId",
                table: "Reservations");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "Staffs");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Roles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    GoogleId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsPasswordSet = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 1,
                column: "UserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 2,
                column: "UserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 3,
                column: "UserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 4,
                column: "UserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 5,
                column: "UserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 6,
                column: "UserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 7,
                column: "UserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 8,
                column: "UserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 9,
                column: "UserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 10,
                column: "UserId",
                value: null);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Address", "CreatedAt", "DateOfBirth", "DeletedAt", "Email", "FullName", "GoogleId", "IsActive", "IsDeleted", "IsPasswordSet", "LastLoginAt", "PasswordHash", "Phone", "RefreshToken", "RefreshTokenExpiry", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { 1, "Hanoi, Vietnam", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1995, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "admin@lms.com", "System Administrator", null, true, false, true, null, "AQAAAAIAAYagAAAAEEFkbWluU2FsdDEyMzQ1Njel4yPDX+DW0RDQGojuqKgDm9LCHqF6JL2jjZxcKy0h1A==", "0900000001", null, null, null, "admin" },
                    { 2, "Ho Chi Minh City", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1994, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "librarian1@lms.com", "Nguyen Van A", null, true, false, true, null, "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==", "0900000002", null, null, null, "librarian1" },
                    { 3, "Da Nang", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2000, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member1@mail.com", "Tran Thi B", null, true, false, true, null, "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==", "0900000003", null, null, null, "member1" },
                    { 4, "Hai Phong", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1999, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member2@mail.com", "Le Van C", null, true, false, true, null, "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==", "0900000004", null, null, null, "member2" },
                    { 5, "Can Tho", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2001, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member3@mail.com", "Pham Thi D", null, true, false, true, null, "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==", "0900000005", null, null, null, "member3" },
                    { 6, "Quang Ninh", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1998, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member4@mail.com", "Hoang Van E", null, true, false, true, null, "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==", "0900000006", null, null, null, "member4" },
                    { 7, "Hue", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2002, 2, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member5@mail.com", "Do Thi F", null, true, false, true, null, "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==", "0900000007", null, null, null, "member5" },
                    { 8, "Binh Duong", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1997, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member6@mail.com", "Vu Van G", null, true, false, true, null, "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==", "0900000008", null, null, null, "member6" },
                    { 9, "Dong Nai", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2003, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member7@mail.com", "Dang Thi H", null, true, false, true, null, "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==", "0900000009", null, null, null, "member7" },
                    { 10, "Nam Dinh", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1996, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member8@mail.com", "Bui Van I", null, true, false, true, null, "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==", "0900000010", null, null, null, "member8" }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId", "CreatedAt", "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null },
                    { 3, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null },
                    { 3, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null },
                    { 3, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null },
                    { 3, 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null },
                    { 3, 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null },
                    { 3, 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null },
                    { 3, 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null },
                    { 2, 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null },
                    { 3, 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_UserId",
                table: "Roles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BookReviews_Users_UserId",
                table: "BookReviews",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowTransactions_Users_UserId",
                table: "BorrowTransactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_Users_UserId",
                table: "Notification",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_UserId",
                table: "Payments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_UserId",
                table: "Reservations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Users_UserId",
                table: "Roles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
