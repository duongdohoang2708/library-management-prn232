using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LibraryManagementDAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIRequestLogs",
                columns: table => new
                {
                    AIRequestLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    ModelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokensUsed = table.Column<int>(type: "int", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIRequestLogs", x => x.AIRequestLogId);
                });

            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    AuthorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Biography = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.AuthorId);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Publishers",
                columns: table => new
                {
                    PublisherId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublisherName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publishers", x => x.PublisherId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "AIRequestLogDetails",
                columns: table => new
                {
                    AIRequestLogDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AIRequestLogId = table.Column<int>(type: "int", nullable: false),
                    Prompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIRequestLogDetails", x => x.AIRequestLogDetailId);
                    table.ForeignKey(
                        name: "FK_AIRequestLogDetails_AIRequestLogs_AIRequestLogId",
                        column: x => x.AIRequestLogId,
                        principalTable: "AIRequestLogs",
                        principalColumn: "AIRequestLogId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ISBN = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PublishYear = table.Column<int>(type: "int", nullable: true),
                    EditionNumber = table.Column<int>(type: "int", nullable: false),
                    ReprintYear = table.Column<int>(type: "int", nullable: true),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    PublisherId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.BookId);
                    table.ForeignKey(
                        name: "FK_Books_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "AuthorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Books_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Books_Publishers_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "Publishers",
                        principalColumn: "PublisherId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BorrowTransactions",
                columns: table => new
                {
                    BorrowTransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BorrowDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowTransactions", x => x.BorrowTransactionId);
                    table.ForeignKey(
                        name: "FK_BorrowTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    IsSent = table.Column<bool>(type: "bit", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notification_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                    table.ForeignKey(
                        name: "FK_Roles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "BookAISummary",
                columns: table => new
                {
                    BookAISummaryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    SummaryText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokensUsed = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookAISummary", x => x.BookAISummaryId);
                    table.ForeignKey(
                        name: "FK_BookAISummary_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookCopies",
                columns: table => new
                {
                    BookCopyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Condition = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCopies", x => x.BookCopyId);
                    table.ForeignKey(
                        name: "FK_BookCopies_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookReviews",
                columns: table => new
                {
                    BookReviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookReviews", x => x.BookReviewId);
                    table.ForeignKey(
                        name: "FK_BookReviews_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookReviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BorrowTransactionId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    TransactionCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_BorrowTransactions_BorrowTransactionId",
                        column: x => x.BorrowTransactionId,
                        principalTable: "BorrowTransactions",
                        principalColumn: "BorrowTransactionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "BorrowDetails",
                columns: table => new
                {
                    BorrowDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BorrowTransactionId = table.Column<int>(type: "int", nullable: false),
                    BookCopyId = table.Column<int>(type: "int", nullable: true),
                    BorrowDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FineAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    FinePaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsFinePaid = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowDetails", x => x.BorrowDetailId);
                    table.ForeignKey(
                        name: "FK_BorrowDetails_BookCopies_BookCopyId",
                        column: x => x.BookCopyId,
                        principalTable: "BookCopies",
                        principalColumn: "BookCopyId");
                    table.ForeignKey(
                        name: "FK_BorrowDetails_BorrowTransactions_BorrowTransactionId",
                        column: x => x.BorrowTransactionId,
                        principalTable: "BorrowTransactions",
                        principalColumn: "BorrowTransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    ReservationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    BookCopyId = table.Column<int>(type: "int", nullable: true),
                    ReservedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.ReservationId);
                    table.ForeignKey(
                        name: "FK_Reservations_BookCopies_BookCopyId",
                        column: x => x.BookCopyId,
                        principalTable: "BookCopies",
                        principalColumn: "BookCopyId");
                    table.ForeignKey(
                        name: "FK_Reservations_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentDetails",
                columns: table => new
                {
                    PaymentDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    BorrowDetailId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentDetails", x => x.PaymentDetailId);
                    table.ForeignKey(
                        name: "FK_PaymentDetails_BorrowDetails_BorrowDetailId",
                        column: x => x.BorrowDetailId,
                        principalTable: "BorrowDetails",
                        principalColumn: "BorrowDetailId");
                    table.ForeignKey(
                        name: "FK_PaymentDetails_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Authors",
                columns: new[] { "AuthorId", "Biography", "CreatedAt", "DeletedAt", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Nhà văn Việt Nam nổi tiếng với các tác phẩm dành cho tuổi học trò.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Nguyễn Nhật Ánh", null },
                    { 2, "Nhà văn người Brazil, tác giả của tiểu thuyết nổi tiếng 'Nhà giả kim'.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Paulo Coelho", null },
                    { 3, "Nhà văn Nhật Bản nổi tiếng với phong cách siêu thực và hiện đại.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Haruki Murakami", null },
                    { 4, "Tác giả bộ truyện giả tưởng Harry Potter nổi tiếng toàn cầu.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "J.K. Rowling", null },
                    { 5, "Nhà văn Mỹ chuyên viết tiểu thuyết trinh thám và ly kỳ.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Dan Brown", null },
                    { 6, "Nhà văn Việt Nam, tác giả tác phẩm 'Dế Mèn Phiêu Lưu Ký'.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Tô Hoài", null },
                    { 7, "Nhà văn hiện thực Việt Nam với các tác phẩm như 'Chí Phèo'.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Nam Cao", null },
                    { 8, "Nhà văn người Anh, nổi tiếng với '1984' và 'Animal Farm'.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "George Orwell", null },
                    { 9, "Nhà văn Mỹ chuyên thể loại kinh dị và tâm lý.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Stephen King", null },
                    { 10, "Nữ hoàng truyện trinh thám với nhân vật thám tử Hercule Poirot.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Agatha Christie", null }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "CategoryName", "CreatedAt", "DeletedAt", "Description", "IsDeleted", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Novel", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "General fiction novels", false, null },
                    { 2, "Fantasy", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Fantasy stories with magical elements", false, null },
                    { 3, "Mystery", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Detective and mystery investigation stories", false, null },
                    { 4, "Science Fiction", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Futuristic science and technology based fiction", false, null },
                    { 5, "Romance", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Love and relationship focused stories", false, null },
                    { 6, "Horror", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Horror and supernatural thriller stories", false, null },
                    { 7, "Classic", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Classic literature works", false, null },
                    { 8, "Adventure", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Adventure and exploration stories", false, null },
                    { 9, "Self-help", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Self development and motivational books", false, null },
                    { 10, "Biography", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Biographies and life stories", false, null }
                });

            migrationBuilder.InsertData(
                table: "Publishers",
                columns: new[] { "PublisherId", "Address", "CreatedAt", "DeletedAt", "IsDeleted", "PublisherName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Ho Chi Minh City, Vietnam", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "NXB Trẻ", null },
                    { 2, "Hanoi, Vietnam", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "NXB Kim Đồng", null },
                    { 3, "Hanoi, Vietnam", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "NXB Văn Học", null },
                    { 4, "London, United Kingdom", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Penguin Books", null },
                    { 5, "New York, USA", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "HarperCollins", null },
                    { 6, "New York, USA", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Random House", null },
                    { 7, "Hanoi, Vietnam", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "NXB Giáo Dục", null },
                    { 8, "Oxford, United Kingdom", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Oxford Press", null },
                    { 9, "London, United Kingdom", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Bloomsbury", null },
                    { 10, "New York, USA", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Simon & Schuster", null }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "CreatedAt", "DeletedAt", "IsDeleted", "RoleName", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Admin", null, null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Librarian", null, null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Member", null, null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Guest", null, null },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Manager", null, null },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Staff", null, null },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Support", null, null },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Auditor", null, null },
                    { 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "ContentEditor", null, null },
                    { 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "SuperAdmin", null, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Address", "CreatedAt", "DateOfBirth", "DeletedAt", "Email", "FullName", "GoogleId", "IsActive", "IsDeleted", "IsPasswordSet", "LastLoginAt", "PasswordHash", "Phone", "RefreshToken", "RefreshTokenExpiry", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { 1, "Hanoi, Vietnam", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1995, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "admin@lms.com", "System Administrator", null, true, false, true, null, "AQAAAAIAAYagAAAAEKPJtWCvwm5R5QfZL3SXwITmroUnaebcZGDz3/NKSCVVyH3w+3J+3mwqj2OhCoFlfA==", "0900000001", null, null, null, "admin" },
                    { 2, "Ho Chi Minh City", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1994, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "librarian1@lms.com", "Nguyen Van A", null, true, false, true, null, "AQAAAAIAAYagAAAAEJgMHGFoOFEqNr0QVviq/LXCJgZT1HVQ6OGxnFhtwnrMfbmqvtd+yiNtP00W58EQNA==", "0900000002", null, null, null, "librarian1" },
                    { 3, "Da Nang", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2000, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member1@mail.com", "Tran Thi B", null, true, false, true, null, "AQAAAAIAAYagAAAAEJgMHGFoOFEqNr0QVviq/LXCJgZT1HVQ6OGxnFhtwnrMfbmqvtd+yiNtP00W58EQNA==", "0900000003", null, null, null, "member1" },
                    { 4, "Hai Phong", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1999, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member2@mail.com", "Le Van C", null, true, false, true, null, "AQAAAAIAAYagAAAAEJgMHGFoOFEqNr0QVviq/LXCJgZT1HVQ6OGxnFhtwnrMfbmqvtd+yiNtP00W58EQNA==", "0900000004", null, null, null, "member2" },
                    { 5, "Can Tho", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2001, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member3@mail.com", "Pham Thi D", null, true, false, true, null, "AQAAAAIAAYagAAAAEJgMHGFoOFEqNr0QVviq/LXCJgZT1HVQ6OGxnFhtwnrMfbmqvtd+yiNtP00W58EQNA==", "0900000005", null, null, null, "member3" },
                    { 6, "Quang Ninh", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1998, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member4@mail.com", "Hoang Van E", null, true, false, true, null, "AQAAAAIAAYagAAAAEJgMHGFoOFEqNr0QVviq/LXCJgZT1HVQ6OGxnFhtwnrMfbmqvtd+yiNtP00W58EQNA==", "0900000006", null, null, null, "member4" },
                    { 7, "Hue", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2002, 2, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member5@mail.com", "Do Thi F", null, true, false, true, null, "AQAAAAIAAYagAAAAEJgMHGFoOFEqNr0QVviq/LXCJgZT1HVQ6OGxnFhtwnrMfbmqvtd+yiNtP00W58EQNA==", "0900000007", null, null, null, "member5" },
                    { 8, "Binh Duong", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1997, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member6@mail.com", "Vu Van G", null, true, false, true, null, "AQAAAAIAAYagAAAAEJgMHGFoOFEqNr0QVviq/LXCJgZT1HVQ6OGxnFhtwnrMfbmqvtd+yiNtP00W58EQNA==", "0900000008", null, null, null, "member6" },
                    { 9, "Dong Nai", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2003, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member7@mail.com", "Dang Thi H", null, true, false, true, null, "AQAAAAIAAYagAAAAEJgMHGFoOFEqNr0QVviq/LXCJgZT1HVQ6OGxnFhtwnrMfbmqvtd+yiNtP00W58EQNA==", "0900000009", null, null, null, "member7" },
                    { 10, "Nam Dinh", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1996, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "member8@mail.com", "Bui Van I", null, true, false, true, null, "AQAAAAIAAYagAAAAEJgMHGFoOFEqNr0QVviq/LXCJgZT1HVQ6OGxnFhtwnrMfbmqvtd+yiNtP00W58EQNA==", "0900000010", null, null, null, "member8" }
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "BookId", "AuthorId", "CategoryId", "CreatedAt", "DeletedAt", "Description", "EditionNumber", "ISBN", "ImageUrl", "IsActive", "IsDeleted", "PublishYear", "PublisherId", "ReprintYear", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9786041000001", null, true, false, 2008, 1, null, "Cho tôi xin một vé đi tuổi thơ", null },
                    { 2, 2, 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780061122415", null, true, false, 1988, 4, null, "Nhà giả kim", null },
                    { 3, 3, 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780375704024", null, true, false, 1987, 6, null, "Rừng Na Uy", null },
                    { 4, 4, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780747532743", null, true, false, 1997, 9, null, "Harry Potter", null },
                    { 5, 5, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780307474278", null, true, false, 2003, 5, null, "Mật mã Da Vinci", null },
                    { 6, 6, 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9786041000018", null, true, false, 1941, 2, null, "Dế Mèn Phiêu Lưu Ký", null },
                    { 7, 7, 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9786041000025", null, true, false, 1941, 3, null, "Chí Phèo", null },
                    { 8, 8, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780451524935", null, true, false, 1949, 8, null, "1984", null },
                    { 9, 9, 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780307743657", null, true, false, 1977, 6, null, "The Shining", null },
                    { 10, 10, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780007119318", null, true, false, 1934, 10, null, "Án mạng trên chuyến tàu tốc hành", null }
                });

            migrationBuilder.InsertData(
                table: "BorrowTransactions",
                columns: new[] { "BorrowTransactionId", "BorrowDate", "CreatedAt", "DeletedAt", "DueDate", "IsDeleted", "Status", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), false, "Returned", null, 2 },
                    { 2, new DateTime(2025, 12, 20, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), false, "Returned", null, 3 },
                    { 3, new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), false, "Borrowing", null, 4 },
                    { 4, new DateTime(2025, 12, 17, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 25, 0, 0, 0, 0, DateTimeKind.Utc), false, "Overdue", null, 5 },
                    { 5, new DateTime(2025, 12, 12, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), false, "Lost", null, 6 },
                    { 6, new DateTime(2025, 12, 24, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), false, "Damaged", null, 7 },
                    { 7, new DateTime(2025, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), false, "Borrowing", null, 8 },
                    { 8, new DateTime(2025, 12, 25, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), false, "Returned", null, 9 },
                    { 9, new DateTime(2025, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), false, "Borrowing", null, 10 },
                    { 10, new DateTime(2025, 12, 2, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 12, 0, 0, 0, 0, DateTimeKind.Utc), false, "Returned", null, 2 },
                    { 101, new DateTime(2025, 12, 12, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), false, "Overdue", null, 4 },
                    { 102, new DateTime(2025, 12, 17, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), false, "Lost", null, 8 },
                    { 103, new DateTime(2025, 12, 20, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), false, "Damaged", null, 9 }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId", "CreatedAt", "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 3, 27, 16, 14, 11, 680, DateTimeKind.Utc).AddTicks(9742), null, false, null },
                    { 3, 2, new DateTime(2026, 3, 27, 16, 14, 11, 680, DateTimeKind.Utc).AddTicks(9744), null, false, null },
                    { 3, 3, new DateTime(2026, 3, 27, 16, 14, 11, 680, DateTimeKind.Utc).AddTicks(9745), null, false, null },
                    { 3, 4, new DateTime(2026, 3, 27, 16, 14, 11, 680, DateTimeKind.Utc).AddTicks(9746), null, false, null },
                    { 3, 5, new DateTime(2026, 3, 27, 16, 14, 11, 680, DateTimeKind.Utc).AddTicks(9747), null, false, null },
                    { 3, 6, new DateTime(2026, 3, 27, 16, 14, 11, 680, DateTimeKind.Utc).AddTicks(9747), null, false, null },
                    { 3, 7, new DateTime(2026, 3, 27, 16, 14, 11, 680, DateTimeKind.Utc).AddTicks(9748), null, false, null },
                    { 3, 8, new DateTime(2026, 3, 27, 16, 14, 11, 680, DateTimeKind.Utc).AddTicks(9749), null, false, null },
                    { 2, 9, new DateTime(2026, 3, 27, 16, 14, 11, 680, DateTimeKind.Utc).AddTicks(9749), null, false, null },
                    { 3, 10, new DateTime(2026, 3, 27, 16, 14, 11, 680, DateTimeKind.Utc).AddTicks(9750), null, false, null }
                });

            migrationBuilder.InsertData(
                table: "BookCopies",
                columns: new[] { "BookCopyId", "Barcode", "BookId", "Condition", "CreatedAt", "DeletedAt", "IsDeleted", "Location", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "BC001", 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "A1", 0, null },
                    { 2, "BC002", 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "A1", 1, null },
                    { 3, "BC003", 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "A2", 2, null },
                    { 4, "BC004", 1, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "A3", 3, null },
                    { 5, "BC005", 1, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "A4", 4, null },
                    { 6, "BC006", 2, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "B1", 0, null },
                    { 7, "BC007", 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "B2", 0, null },
                    { 8, "BC008", 2, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "B3", 1, null },
                    { 9, "BC009", 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "B4", 2, null },
                    { 10, "BC010", 2, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "B5", 0, null },
                    { 11, "BC011", 3, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "C1", 0, null },
                    { 12, "BC012", 3, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "C1", 1, null },
                    { 13, "BC013", 3, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "C2", 2, null },
                    { 14, "BC014", 3, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "C3", 3, null },
                    { 15, "BC015", 3, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "C4", 4, null },
                    { 16, "BC016", 4, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "D1", 0, null },
                    { 17, "BC017", 4, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "D2", 0, null },
                    { 18, "BC018", 4, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "D3", 1, null },
                    { 19, "BC019", 4, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "D4", 2, null },
                    { 20, "BC020", 4, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "D5", 0, null }
                });

            migrationBuilder.InsertData(
                table: "BookReviews",
                columns: new[] { "BookReviewId", "BookId", "Comment", "CreatedAt", "DeletedAt", "IsDeleted", "Rating", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 3, 1, "Rất hay", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 5, null, 2 },
                    { 4, 1, "Đọc lại vẫn xúc động", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 4, null, 3 },
                    { 5, 1, "Khá ổn", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 3, null, 4 },
                    { 6, 1, "Tuổi thơ ùa về", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 5, null, 5 },
                    { 7, 2, "Tái bản đẹp", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 4, null, 6 },
                    { 8, 2, "Nội dung hơi chậm", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 2, null, 7 },
                    { 9, 2, "Tuyệt vời", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 5, null, 8 },
                    { 10, 2, "Đáng đọc", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 4, null, 9 },
                    { 11, 1, "Xuất sắc", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 5, null, 10 },
                    { 12, 2, "Phiên bản mới đẹp hơn", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 4, null, 2 }
                });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "PaymentId", "Amount", "BorrowTransactionId", "CreatedAt", "DeletedAt", "IsDeleted", "PaidAt", "PaymentMethod", "PaymentStatus", "TransactionCode", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1, 5000m, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, 1, null, null, 2 },
                    { 2, 10000m, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, 1, null, null, 3 },
                    { 3, 20000m, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, 3, 0, null, null, 4 },
                    { 4, 15000m, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, 0, 2, null, null, 5 },
                    { 5, 7000m, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, 1, null, null, 6 },
                    { 6, 9000m, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, 0, 3, null, null, 7 },
                    { 7, 3000m, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 1, null, null, 8 },
                    { 8, 4500m, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, 0, 0, null, null, 9 },
                    { 9, 8000m, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, 1, null, null, 10 },
                    { 10, 12000m, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, 1, null, null, 2 },
                    { 11, 5000m, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, 1, null, null, 3 },
                    { 12, 100000m, 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, 1, null, null, 6 },
                    { 13, 50000m, 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, 0, 0, null, null, 7 }
                });

            migrationBuilder.InsertData(
                table: "Reservations",
                columns: new[] { "ReservationId", "BookCopyId", "BookId", "CreatedAt", "DeletedAt", "ExpireAt", "IsDeleted", "ReservedAt", "Status", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1, null, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, null, 2 },
                    { 2, null, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, 3 },
                    { 3, null, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, 4 },
                    { 4, null, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 5 },
                    { 5, null, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, 6 },
                    { 6, null, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, null, 7 },
                    { 7, null, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, 8 },
                    { 8, null, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, 9 },
                    { 9, null, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, 10 },
                    { 10, null, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, null, 2 }
                });

            migrationBuilder.InsertData(
                table: "BorrowDetails",
                columns: new[] { "BorrowDetailId", "ActualReturnDate", "BookCopyId", "BorrowDate", "BorrowTransactionId", "CreatedAt", "DeletedAt", "DueDate", "FineAmount", "FinePaidAmount", "IsDeleted", "IsFinePaid", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), 1, new DateTime(2025, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), 0m, null, false, false, null },
                    { 2, new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), 2, new DateTime(2025, 12, 20, 0, 0, 0, 0, DateTimeKind.Utc), 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), 5000m, null, false, false, null },
                    { 3, null, 3, new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), 0m, null, false, false, null },
                    { 4, null, 4, new DateTime(2025, 12, 17, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 25, 0, 0, 0, 0, DateTimeKind.Utc), 3000m, null, false, false, null },
                    { 5, null, 5, new DateTime(2025, 12, 12, 0, 0, 0, 0, DateTimeKind.Utc), 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), 100000m, null, false, false, null },
                    { 6, new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), 6, new DateTime(2025, 12, 24, 0, 0, 0, 0, DateTimeKind.Utc), 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), 50000m, null, false, false, null },
                    { 7, null, 7, new DateTime(2025, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), 0m, null, false, false, null },
                    { 8, new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), 8, new DateTime(2025, 12, 25, 0, 0, 0, 0, DateTimeKind.Utc), 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), 0m, null, false, false, null },
                    { 9, null, 9, new DateTime(2025, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), 0m, null, false, false, null },
                    { 10, new DateTime(2025, 12, 12, 0, 0, 0, 0, DateTimeKind.Utc), 10, new DateTime(2025, 12, 2, 0, 0, 0, 0, DateTimeKind.Utc), 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 12, 0, 0, 0, 0, DateTimeKind.Utc), 0m, null, false, false, null },
                    { 101, null, 11, new DateTime(2025, 12, 12, 0, 0, 0, 0, DateTimeKind.Utc), 101, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), 50000m, null, false, false, null },
                    { 102, null, 12, new DateTime(2025, 12, 17, 0, 0, 0, 0, DateTimeKind.Utc), 102, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), 100000m, null, false, false, null },
                    { 103, null, 13, new DateTime(2025, 12, 20, 0, 0, 0, 0, DateTimeKind.Utc), 103, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), 50000m, null, false, false, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIRequestLogDetails_AIRequestLogId",
                table: "AIRequestLogDetails",
                column: "AIRequestLogId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookAISummary_BookId",
                table: "BookAISummary",
                column: "BookId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookCopies_Barcode",
                table: "BookCopies",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookCopies_BookId",
                table: "BookCopies",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookReviews_BookId",
                table: "BookReviews",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookReviews_UserId",
                table: "BookReviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_AuthorId",
                table: "Books",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_CategoryId",
                table: "Books",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_ISBN",
                table: "Books",
                column: "ISBN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Books_PublisherId",
                table: "Books",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowDetails_BookCopyId",
                table: "BorrowDetails",
                column: "BookCopyId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowDetails_BorrowTransactionId",
                table: "BorrowDetails",
                column: "BorrowTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowTransactions_UserId",
                table: "BorrowTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId",
                table: "Notification",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetails_BorrowDetailId",
                table: "PaymentDetails",
                column: "BorrowDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetails_PaymentId",
                table: "PaymentDetails",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BorrowTransactionId",
                table: "Payments",
                column: "BorrowTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BookCopyId",
                table: "Reservations",
                column: "BookCopyId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BookId_Status",
                table: "Reservations",
                columns: new[] { "BookId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId",
                table: "Reservations",
                column: "UserId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIRequestLogDetails");

            migrationBuilder.DropTable(
                name: "BookAISummary");

            migrationBuilder.DropTable(
                name: "BookReviews");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "PaymentDetails");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "AIRequestLogs");

            migrationBuilder.DropTable(
                name: "BorrowDetails");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "BookCopies");

            migrationBuilder.DropTable(
                name: "BorrowTransactions");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Publishers");
        }
    }
}
