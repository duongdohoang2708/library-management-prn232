using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddRenewalRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RenewalRequests",
                columns: table => new
                {
                    RenewalRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BorrowDetailId = table.Column<int>(type: "int", nullable: false),
                    RequestedExtraDays = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NewDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenewalRequests", x => x.RenewalRequestId);
                    table.ForeignKey(
                        name: "FK_RenewalRequests_Accounts_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Accounts",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RenewalRequests_Accounts_UserId",
                        column: x => x.UserId,
                        principalTable: "Accounts",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RenewalRequests_BorrowDetails_BorrowDetailId",
                        column: x => x.BorrowDetailId,
                        principalTable: "BorrowDetails",
                        principalColumn: "BorrowDetailId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RenewalRequests_BorrowDetailId_Status",
                table: "RenewalRequests",
                columns: new[] { "BorrowDetailId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RenewalRequests_ReviewedByUserId",
                table: "RenewalRequests",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RenewalRequests_UserId",
                table: "RenewalRequests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RenewalRequests");
        }
    }
}
