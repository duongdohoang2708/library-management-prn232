using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LibraryManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Authors",
                columns: new[] { "AuthorId", "Biography", "CreatedAt", "DeletedAt", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 11, "Nhà tiểu sử học người Mỹ, tác giả tiểu sử Steve Jobs, Einstein.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Walter Isaacson", null },
                    { 12, "Nhà văn và phi công người Pháp, tác giả 'Hoàng tử bé'.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Antoine de Saint-Exupéry", null },
                    { 13, "Nhà văn người Scotland nổi tiếng với thám tử Sherlock Holmes.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Arthur Conan Doyle", null },
                    { 14, "Nhà văn và nhà thuyết trình người Mỹ, tác giả Đắc Nhân Tâm.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Dale Carnegie", null },
                    { 15, "Nhà văn Pháp đi tiên phong trong thể loại khoa học viễn tưởng.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Jules Verne", null },
                    { 16, "Nữ văn sĩ người Anh với những tác phẩm kinh điển.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Jane Austen", null },
                    { 17, "Nhà văn Mỹ nổi tiếng với tiểu thuyết tình cảm lãng mạn.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Nicholas Sparks", null },
                    { 18, "Nhà văn, nhà báo hiện thực xuất sắc của văn học Việt Nam.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Ngô Tất Tố", null },
                    { 19, "Nhà văn Việt Nam với các tác phẩm viết về thiên nhiên Nam Bộ.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Đoàn Giỏi", null }
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "BookId", "AuthorId", "CategoryId", "CreatedAt", "DeletedAt", "Description", "EditionNumber", "ISBN", "ImageUrl", "IsActive", "IsDeleted", "PublishYear", "PublisherId", "ReprintYear", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 11, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9786041000032", null, true, false, 1990, 1, null, "Mắt biếc", null },
                    { 12, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9786041000049", null, true, false, 1989, 1, null, "Cô gái đến từ hôm qua", null },
                    { 13, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9786041000056", null, true, false, 2010, 1, null, "Tôi thấy hoa vàng trên cỏ xanh", null },
                    { 14, 4, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780747538493", null, true, false, 1998, 9, null, "Harry Potter và Phòng chứa Bí mật", null },
                    { 15, 4, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780747542155", null, true, false, 1999, 9, null, "Harry Potter và Tên tù nhân ngục Azkaban", null },
                    { 18, 10, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780007119325", null, true, false, 1937, 10, null, "Vụ án mạng dòng sông Nile", null },
                    { 19, 5, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780307474285", null, true, false, 2009, 5, null, "Biểu tượng thất truyền", null },
                    { 24, 3, 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780099448570", null, true, false, 1992, 6, null, "Phía Nam biên giới, phía Tây mặt trời", null },
                    { 26, 9, 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9781501142970", null, true, false, 1986, 6, null, "It (Chú hề ma quái)", null },
                    { 27, 9, 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9781501156700", null, true, false, 1983, 6, null, "Pet Sematary (Nghĩa địa thú cưng)", null },
                    { 28, 9, 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9781501156748", null, true, false, 1987, 6, null, "Misery", null },
                    { 29, 7, 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9786041000063", null, true, false, 1943, 3, null, "Lão Hạc", null },
                    { 30, 7, 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9786041000070", null, true, false, 1944, 3, null, "Sống mòn", null },
                    { 33, 6, 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9786041000100", null, true, false, 1952, 2, null, "Vợ chồng A Phủ", null },
                    { 34, 6, 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9786041000117", null, true, false, 1960, 2, null, "Tre Phong Ba", null },
                    { 37, 2, 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9781454900672", null, true, false, 2011, 4, null, "Chiến thắng con quỷ trong bạn", null }
                });

            migrationBuilder.InsertData(
                table: "BookCopies",
                columns: new[] { "BookCopyId", "Barcode", "BookId", "Condition", "CreatedAt", "DeletedAt", "IsDeleted", "Location", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { 21, "BC021", 11, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 1", 0, null },
                    { 22, "BC022", 11, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 1", 0, null },
                    { 23, "BC023", 12, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 1", 0, null },
                    { 24, "BC024", 12, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 1", 0, null },
                    { 25, "BC025", 13, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 1", 0, null },
                    { 26, "BC026", 13, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 1", 0, null },
                    { 27, "BC027", 14, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 2", 0, null },
                    { 28, "BC028", 14, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 2", 0, null },
                    { 29, "BC029", 15, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 2", 0, null },
                    { 30, "BC030", 15, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 2", 0, null },
                    { 35, "BC035", 18, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 3", 0, null },
                    { 36, "BC036", 18, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 3", 0, null },
                    { 37, "BC037", 19, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 3", 0, null },
                    { 38, "BC038", 19, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 3", 0, null },
                    { 47, "BC047", 24, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 5", 0, null },
                    { 48, "BC048", 24, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 5", 0, null },
                    { 51, "BC051", 26, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 6", 0, null },
                    { 52, "BC052", 26, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 6", 0, null },
                    { 53, "BC053", 27, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 6", 0, null },
                    { 54, "BC054", 27, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 6", 0, null },
                    { 55, "BC055", 28, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 6", 0, null },
                    { 56, "BC056", 28, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 6", 0, null },
                    { 57, "BC057", 29, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 7", 0, null },
                    { 58, "BC058", 29, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 7", 0, null },
                    { 59, "BC059", 30, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 7", 0, null },
                    { 60, "BC060", 30, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 7", 0, null },
                    { 65, "BC065", 33, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 8", 0, null },
                    { 66, "BC066", 33, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 8", 0, null },
                    { 67, "BC067", 34, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 8", 0, null },
                    { 68, "BC068", 34, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 8", 0, null },
                    { 73, "BC073", 37, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 9", 0, null },
                    { 74, "BC074", 37, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 9", 0, null }
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "BookId", "AuthorId", "CategoryId", "CreatedAt", "DeletedAt", "Description", "EditionNumber", "ISBN", "ImageUrl", "IsActive", "IsDeleted", "PublishYear", "PublisherId", "ReprintYear", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 16, 12, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780156012195", null, true, false, 1943, 4, null, "Hoàng tử bé", null },
                    { 17, 13, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780141036137", null, true, false, 1887, 4, null, "Sherlock Holmes - Chiếc nhẫn tình cờ", null },
                    { 20, 15, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780553212525", null, true, false, 1870, 8, null, "Hai vạn dặm dưới đáy biển", null },
                    { 21, 15, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780140624250", null, true, false, 1864, 8, null, "Hành trình vào tâm Trái Đất", null },
                    { 22, 15, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780143125754", null, true, false, 1865, 8, null, "Từ Trái Đất lên Mặt Trăng", null },
                    { 23, 16, 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780141439518", null, true, false, 1813, 4, null, "Kiêu hãnh và Định kiến", null },
                    { 25, 17, 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780446605236", null, true, false, 1996, 5, null, "Nhật ký son trẻ", null },
                    { 31, 18, 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9786041000087", null, true, false, 1937, 3, null, "Tắt đèn", null },
                    { 32, 19, 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9786041000094", null, true, false, 1957, 2, null, "Đất rừng phương Nam", null },
                    { 35, 14, 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780671733353", null, true, false, 1936, 10, null, "Đắc Nhân Tâm", null },
                    { 36, 14, 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780671733360", null, true, false, 1948, 10, null, "Quẳng gánh lo đi và vui sống", null },
                    { 38, 11, 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9781451648539", null, true, false, 2011, 10, null, "Steve Jobs", null },
                    { 39, 11, 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9780743264747", null, true, false, 2007, 10, null, "Einstein: Cuộc đời và Vũ trụ", null },
                    { 40, 11, 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "9781501139154", null, true, false, 2017, 10, null, "Leonardo da Vinci", null }
                });

            migrationBuilder.InsertData(
                table: "BookCopies",
                columns: new[] { "BookCopyId", "Barcode", "BookId", "Condition", "CreatedAt", "DeletedAt", "IsDeleted", "Location", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { 31, "BC031", 16, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 2", 0, null },
                    { 32, "BC032", 16, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 2", 0, null },
                    { 33, "BC033", 17, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 3", 0, null },
                    { 34, "BC034", 17, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 3", 0, null },
                    { 39, "BC039", 20, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 4", 0, null },
                    { 40, "BC040", 20, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 4", 0, null },
                    { 41, "BC041", 21, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 4", 0, null },
                    { 42, "BC042", 21, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 4", 0, null },
                    { 43, "BC043", 22, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 4", 0, null },
                    { 44, "BC044", 22, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 4", 0, null },
                    { 45, "BC045", 23, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 5", 0, null },
                    { 46, "BC046", 23, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 5", 0, null },
                    { 49, "BC049", 25, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 5", 0, null },
                    { 50, "BC050", 25, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 5", 0, null },
                    { 61, "BC061", 31, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 7", 0, null },
                    { 62, "BC062", 31, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 7", 0, null },
                    { 63, "BC063", 32, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 8", 0, null },
                    { 64, "BC064", 32, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 8", 0, null },
                    { 69, "BC069", 35, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 9", 0, null },
                    { 70, "BC070", 35, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 9", 0, null },
                    { 71, "BC071", 36, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 9", 0, null },
                    { 72, "BC072", 36, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 9", 0, null },
                    { 75, "BC075", 38, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 10", 0, null },
                    { 76, "BC076", 38, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 10", 0, null },
                    { 77, "BC077", 39, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 10", 0, null },
                    { 78, "BC078", 39, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 10", 0, null },
                    { 79, "BC079", 40, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 10", 0, null },
                    { 80, "BC080", 40, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Shelf 10", 0, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 77);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "AuthorId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "AuthorId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "AuthorId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "AuthorId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "AuthorId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "AuthorId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "AuthorId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "AuthorId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "AuthorId",
                keyValue: 19);
        }
    }
}
