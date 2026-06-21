using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace LibraryManagementDAL.Data
{
    public static class SeedData
    {
        // Hash cố định để EF Migration không bị PendingModelChangesWarning.
        // Không dùng PasswordHasher.HashPassword() trực tiếp trong HasData
        // vì mỗi lần build nó sinh salt khác nhau.
        // Password gốc: Admin@123
        private const string AdminPasswordHash =
            "AQAAAAIAAYagAAAAEEFkbWluU2FsdDEyMzQ1Njel4yPDX+DW0RDQGojuqKgDm9LCHqF6JL2jjZxcKy0h1A==";

        // Password gốc: Member@123
        private const string MemberPasswordHash =
            "AQAAAAIAAYagAAAAEE1lbWJlclNhbHQxMjM0NTbb4nq3kD8GRaid1Zudm+jWWubakx0Gsks/vCV1LxDpBQ==";

        public static void Seed(ModelBuilder modelBuilder)
        {
            // Dùng ngày cố định để migration hash không thay đổi mỗi lần rebuild
            var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // ROLE
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin", CreatedAt = now },
                new Role { RoleId = 2, RoleName = "Librarian", CreatedAt = now },
                new Role { RoleId = 3, RoleName = "Member", CreatedAt = now },
                new Role { RoleId = 4, RoleName = "Guest", CreatedAt = now },
                new Role { RoleId = 5, RoleName = "Manager", CreatedAt = now },
                new Role { RoleId = 6, RoleName = "Staff", CreatedAt = now },
                new Role { RoleId = 7, RoleName = "Support", CreatedAt = now },
                new Role { RoleId = 8, RoleName = "Auditor", CreatedAt = now },
                new Role { RoleId = 9, RoleName = "ContentEditor", CreatedAt = now },
                new Role { RoleId = 10, RoleName = "SuperAdmin", CreatedAt = now }
            );

            // USER
            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    UserId = 1,
                    Username = "admin",
                    Email = "admin@lms.com",
                    PasswordHash = AdminPasswordHash,
                    FullName = "System Administrator",
                    Phone = "0900000001",
                    Address = "Hanoi, Vietnam",
                    DateOfBirth = new DateTime(1995, 1, 1),
                    IsActive = true,
                    LastLoginAt = null,
                    CreatedAt = now
                },
                new Account
                {
                    UserId = 2,
                    Username = "librarian1",
                    Email = "librarian1@lms.com",
                    PasswordHash = MemberPasswordHash,
                    FullName = "Nguyen Van A",
                    Phone = "0900000002",
                    Address = "Ho Chi Minh City",
                    DateOfBirth = new DateTime(1994, 5, 10),
                    IsActive = true,
                    CreatedAt = now
                },
                new Account
                {
                    UserId = 3,
                    Username = "member1",
                    Email = "member1@mail.com",
                    PasswordHash = MemberPasswordHash,
                    FullName = "Tran Thi B",
                    Phone = "0900000003",
                    Address = "Da Nang",
                    DateOfBirth = new DateTime(2000, 3, 12),
                    IsActive = true,
                    CreatedAt = now
                },
                new Account
                {
                    UserId = 4,
                    Username = "member2",
                    Email = "member2@mail.com",
                    PasswordHash = MemberPasswordHash,
                    FullName = "Le Van C",
                    Phone = "0900000004",
                    Address = "Hai Phong",
                    DateOfBirth = new DateTime(1999, 8, 22),
                    IsActive = true,
                    CreatedAt = now
                },
                new Account
                {
                    UserId = 5,
                    Username = "member3",
                    Email = "member3@mail.com",
                    PasswordHash = MemberPasswordHash,
                    FullName = "Pham Thi D",
                    Phone = "0900000005",
                    Address = "Can Tho",
                    DateOfBirth = new DateTime(2001, 6, 15),
                    IsActive = true,
                    CreatedAt = now
                },
                new Account
                {
                    UserId = 6,
                    Username = "member4",
                    Email = "member4@mail.com",
                    PasswordHash = MemberPasswordHash,
                    FullName = "Hoang Van E",
                    Phone = "0900000006",
                    Address = "Quang Ninh",
                    DateOfBirth = new DateTime(1998, 11, 2),
                    IsActive = true,
                    CreatedAt = now
                },
                new Account
                {
                    UserId = 7,
                    Username = "member5",
                    Email = "member5@mail.com",
                    PasswordHash = MemberPasswordHash,
                    FullName = "Do Thi F",
                    Phone = "0900000007",
                    Address = "Hue",
                    DateOfBirth = new DateTime(2002, 2, 18),
                    IsActive = true,
                    CreatedAt = now
                },
                new Account
                {
                    UserId = 8,
                    Username = "member6",
                    Email = "member6@mail.com",
                    PasswordHash = MemberPasswordHash,
                    FullName = "Vu Van G",
                    Phone = "0900000008",
                    Address = "Binh Duong",
                    DateOfBirth = new DateTime(1997, 9, 30),
                    IsActive = true,
                    CreatedAt = now
                },
                new Account
                {
                    UserId = 9,
                    Username = "member7",
                    Email = "member7@mail.com",
                    PasswordHash = MemberPasswordHash,
                    FullName = "Dang Thi H",
                    Phone = "0900000009",
                    Address = "Dong Nai",
                    DateOfBirth = new DateTime(2003, 4, 5),
                    IsActive = true,
                    CreatedAt = now
                },
                new Account
                {
                    UserId = 10,
                    Username = "member8",
                    Email = "member8@mail.com",
                    PasswordHash = MemberPasswordHash,
                    FullName = "Bui Van I",
                    Phone = "0900000010",
                    Address = "Nam Dinh",
                    DateOfBirth = new DateTime(1996, 12, 25),
                    IsActive = true,
                    CreatedAt = now
                }
            );

            modelBuilder.Entity<Member>().HasData(
                new Member { MemberId = 1, UserId = 2, MemberCode = "MEM00001", JoinedAt = now, CreatedAt = now },
                new Member { MemberId = 2, UserId = 3, MemberCode = "MEM00002", JoinedAt = now, CreatedAt = now },
                new Member { MemberId = 3, UserId = 4, MemberCode = "MEM00003", JoinedAt = now, CreatedAt = now },
                new Member { MemberId = 4, UserId = 5, MemberCode = "MEM00004", JoinedAt = now, CreatedAt = now },
                new Member { MemberId = 5, UserId = 6, MemberCode = "MEM00005", JoinedAt = now, CreatedAt = now },
                new Member { MemberId = 6, UserId = 7, MemberCode = "MEM00006", JoinedAt = now, CreatedAt = now },
                new Member { MemberId = 7, UserId = 8, MemberCode = "MEM00007", JoinedAt = now, CreatedAt = now },
                new Member { MemberId = 8, UserId = 10, MemberCode = "MEM00008", JoinedAt = now, CreatedAt = now }
            );

            modelBuilder.Entity<Staff>().HasData(
                new Staff { StaffId = 1, UserId = 1, RoleId = 1, StaffCode = "STF00001", HiredAt = now, CreatedAt = now },
                new Staff { StaffId = 2, UserId = 9, RoleId = 2, StaffCode = "STF00002", HiredAt = now, CreatedAt = now }
            );

            // AUTHOR
            modelBuilder.Entity<Author>().HasData(
                new Author
                {
                    AuthorId = 1,
                    Name = "Nguyễn Nhật Ánh",
                    Biography = "Nhà văn Việt Nam nổi tiếng với các tác phẩm dành cho tuổi học trò.",
                    CreatedAt = now
                },
                new Author
                {
                    AuthorId = 2,
                    Name = "Paulo Coelho",
                    Biography = "Nhà văn người Brazil, tác giả của tiểu thuyết nổi tiếng 'Nhà giả kim'.",
                    CreatedAt = now
                },
                new Author
                {
                    AuthorId = 3,
                    Name = "Haruki Murakami",
                    Biography = "Nhà văn Nhật Bản nổi tiếng với phong cách siêu thực và hiện đại.",
                    CreatedAt = now
                },
                new Author
                {
                    AuthorId = 4,
                    Name = "J.K. Rowling",
                    Biography = "Tác giả bộ truyện giả tưởng Harry Potter nổi tiếng toàn cầu.",
                    CreatedAt = now
                },
                new Author
                {
                    AuthorId = 5,
                    Name = "Dan Brown",
                    Biography = "Nhà văn Mỹ chuyên viết tiểu thuyết trinh thám và ly kỳ.",
                    CreatedAt = now
                },
                new Author
                {
                    AuthorId = 6,
                    Name = "Tô Hoài",
                    Biography = "Nhà văn Việt Nam, tác giả tác phẩm 'Dế Mèn Phiêu Lưu Ký'.",
                    CreatedAt = now
                },
                new Author
                {
                    AuthorId = 7,
                    Name = "Nam Cao",
                    Biography = "Nhà văn hiện thực Việt Nam với các tác phẩm như 'Chí Phèo'.",
                    CreatedAt = now
                },
                new Author
                {
                    AuthorId = 8,
                    Name = "George Orwell",
                    Biography = "Nhà văn người Anh, nổi tiếng với '1984' và 'Animal Farm'.",
                    CreatedAt = now
                },
                new Author
                {
                    AuthorId = 9,
                    Name = "Stephen King",
                    Biography = "Nhà văn Mỹ chuyên thể loại kinh dị và tâm lý.",
                    CreatedAt = now
                },
                new Author
                {
                    AuthorId = 10,
                    Name = "Agatha Christie",
                    Biography = "Nữ hoàng truyện trinh thám với nhân vật thám tử Hercule Poirot.",
                    CreatedAt = now
                }
            );

            // CATEGORY
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Novel", Description = "General fiction novels", CreatedAt = now },
                new Category { CategoryId = 2, CategoryName = "Fantasy", Description = "Fantasy stories with magical elements", CreatedAt = now },
                new Category { CategoryId = 3, CategoryName = "Mystery", Description = "Detective and mystery investigation stories", CreatedAt = now },
                new Category { CategoryId = 4, CategoryName = "Science Fiction", Description = "Futuristic science and technology based fiction", CreatedAt = now },
                new Category { CategoryId = 5, CategoryName = "Romance", Description = "Love and relationship focused stories", CreatedAt = now },
                new Category { CategoryId = 6, CategoryName = "Horror", Description = "Horror and supernatural thriller stories", CreatedAt = now },
                new Category { CategoryId = 7, CategoryName = "Classic", Description = "Classic literature works", CreatedAt = now },
                new Category { CategoryId = 8, CategoryName = "Adventure", Description = "Adventure and exploration stories", CreatedAt = now },
                new Category { CategoryId = 9, CategoryName = "Self-help", Description = "Self development and motivational books", CreatedAt = now },
                new Category { CategoryId = 10, CategoryName = "Biography", Description = "Biographies and life stories", CreatedAt = now }
            );

            // PUBLISHER
            modelBuilder.Entity<Publisher>().HasData(
                new Publisher { PublisherId = 1, PublisherName = "NXB Trẻ", Address = "Ho Chi Minh City, Vietnam", CreatedAt = now },
                new Publisher { PublisherId = 2, PublisherName = "NXB Kim Đồng", Address = "Hanoi, Vietnam", CreatedAt = now },
                new Publisher { PublisherId = 3, PublisherName = "NXB Văn Học", Address = "Hanoi, Vietnam", CreatedAt = now },
                new Publisher { PublisherId = 4, PublisherName = "Penguin Books", Address = "London, United Kingdom", CreatedAt = now },
                new Publisher { PublisherId = 5, PublisherName = "HarperCollins", Address = "New York, USA", CreatedAt = now },
                new Publisher { PublisherId = 6, PublisherName = "Random House", Address = "New York, USA", CreatedAt = now },
                new Publisher { PublisherId = 7, PublisherName = "NXB Giáo Dục", Address = "Hanoi, Vietnam", CreatedAt = now },
                new Publisher { PublisherId = 8, PublisherName = "Oxford Press", Address = "Oxford, United Kingdom", CreatedAt = now },
                new Publisher { PublisherId = 9, PublisherName = "Bloomsbury", Address = "London, United Kingdom", CreatedAt = now },
                new Publisher { PublisherId = 10, PublisherName = "Simon & Schuster", Address = "New York, USA", CreatedAt = now }
            );

            // BOOK
            modelBuilder.Entity<Book>().HasData(
                new Book { BookId = 1, Title = "Cho tôi xin một vé đi tuổi thơ", ISBN = "9786041000001", PublishYear = 2008, EditionNumber = 1, AuthorId = 1, CategoryId = 1, PublisherId = 1, CreatedAt = now, IsActive = true },
                new Book { BookId = 2, Title = "Nhà giả kim", ISBN = "9780061122415", PublishYear = 1988, EditionNumber = 1, AuthorId = 2, CategoryId = 9, PublisherId = 4, CreatedAt = now, IsActive = true },
                new Book { BookId = 3, Title = "Rừng Na Uy", ISBN = "9780375704024", PublishYear = 1987, EditionNumber = 1, AuthorId = 3, CategoryId = 5, PublisherId = 6, CreatedAt = now, IsActive = true },
                new Book { BookId = 4, Title = "Harry Potter", ISBN = "9780747532743", PublishYear = 1997, EditionNumber = 1, AuthorId = 4, CategoryId = 2, PublisherId = 9, CreatedAt = now, IsActive = true },
                new Book { BookId = 5, Title = "Mật mã Da Vinci", ISBN = "9780307474278", PublishYear = 2003, EditionNumber = 1, AuthorId = 5, CategoryId = 3, PublisherId = 5, CreatedAt = now, IsActive = true },
                new Book { BookId = 6, Title = "Dế Mèn Phiêu Lưu Ký", ISBN = "9786041000018", PublishYear = 1941, EditionNumber = 1, AuthorId = 6, CategoryId = 8, PublisherId = 2, CreatedAt = now, IsActive = true },
                new Book { BookId = 7, Title = "Chí Phèo", ISBN = "9786041000025", PublishYear = 1941, EditionNumber = 1, AuthorId = 7, CategoryId = 7, PublisherId = 3, CreatedAt = now, IsActive = true },
                new Book { BookId = 8, Title = "1984", ISBN = "9780451524935", PublishYear = 1949, EditionNumber = 1, AuthorId = 8, CategoryId = 4, PublisherId = 8, CreatedAt = now, IsActive = true },
                new Book { BookId = 9, Title = "The Shining", ISBN = "9780307743657", PublishYear = 1977, EditionNumber = 1, AuthorId = 9, CategoryId = 6, PublisherId = 6, CreatedAt = now, IsActive = true },
                new Book { BookId = 10, Title = "Án mạng trên chuyến tàu tốc hành", ISBN = "9780007119318", PublishYear = 1934, EditionNumber = 1, AuthorId = 10, CategoryId = 3, PublisherId = 10, CreatedAt = now, IsActive = true }
            );

            // BOOK COPY
            modelBuilder.Entity<BookCopy>().HasData(
                new BookCopy { BookCopyId = 1, BookId = 1, Barcode = "BC001", Status = BookCopyStatus.Available, Condition = BookCondition.New, Location = "A1", CreatedAt = now },
                new BookCopy { BookCopyId = 2, BookId = 1, Barcode = "BC002", Status = BookCopyStatus.Borrowed, Condition = BookCondition.Good, Location = "A1", CreatedAt = now },
                new BookCopy { BookCopyId = 3, BookId = 1, Barcode = "BC003", Status = BookCopyStatus.Reserved, Condition = BookCondition.Good, Location = "A2", CreatedAt = now },
                new BookCopy { BookCopyId = 4, BookId = 1, Barcode = "BC004", Status = BookCopyStatus.Lost, Condition = BookCondition.Poor, Location = "A3", CreatedAt = now },
                new BookCopy { BookCopyId = 5, BookId = 1, Barcode = "BC005", Status = BookCopyStatus.Damaged, Condition = BookCondition.Damaged, Location = "A4", CreatedAt = now },
                new BookCopy { BookCopyId = 6, BookId = 2, Barcode = "BC006", Status = BookCopyStatus.Available, Condition = BookCondition.New, Location = "B1", CreatedAt = now },
                new BookCopy { BookCopyId = 7, BookId = 2, Barcode = "BC007", Status = BookCopyStatus.Available, Condition = BookCondition.Good, Location = "B2", CreatedAt = now },
                new BookCopy { BookCopyId = 8, BookId = 2, Barcode = "BC008", Status = BookCopyStatus.Borrowed, Condition = BookCondition.Fair, Location = "B3", CreatedAt = now },
                new BookCopy { BookCopyId = 9, BookId = 2, Barcode = "BC009", Status = BookCopyStatus.Reserved, Condition = BookCondition.Good, Location = "B4", CreatedAt = now },
                new BookCopy { BookCopyId = 10, BookId = 2, Barcode = "BC010", Status = BookCopyStatus.Available, Condition = BookCondition.New, Location = "B5", CreatedAt = now },
                new BookCopy { BookCopyId = 11, BookId = 3, Barcode = "BC011", Status = BookCopyStatus.Available, Condition = BookCondition.New, Location = "C1", CreatedAt = now },
                new BookCopy { BookCopyId = 12, BookId = 3, Barcode = "BC012", Status = BookCopyStatus.Borrowed, Condition = BookCondition.Good, Location = "C1", CreatedAt = now },
                new BookCopy { BookCopyId = 13, BookId = 3, Barcode = "BC013", Status = BookCopyStatus.Reserved, Condition = BookCondition.Good, Location = "C2", CreatedAt = now },
                new BookCopy { BookCopyId = 14, BookId = 3, Barcode = "BC014", Status = BookCopyStatus.Lost, Condition = BookCondition.Poor, Location = "C3", CreatedAt = now },
                new BookCopy { BookCopyId = 15, BookId = 3, Barcode = "BC015", Status = BookCopyStatus.Damaged, Condition = BookCondition.Damaged, Location = "C4", CreatedAt = now },
                new BookCopy { BookCopyId = 16, BookId = 4, Barcode = "BC016", Status = BookCopyStatus.Available, Condition = BookCondition.New, Location = "D1", CreatedAt = now },
                new BookCopy { BookCopyId = 17, BookId = 4, Barcode = "BC017", Status = BookCopyStatus.Available, Condition = BookCondition.Good, Location = "D2", CreatedAt = now },
                new BookCopy { BookCopyId = 18, BookId = 4, Barcode = "BC018", Status = BookCopyStatus.Borrowed, Condition = BookCondition.Fair, Location = "D3", CreatedAt = now },
                new BookCopy { BookCopyId = 19, BookId = 4, Barcode = "BC019", Status = BookCopyStatus.Reserved, Condition = BookCondition.Good, Location = "D4", CreatedAt = now },
                new BookCopy { BookCopyId = 20, BookId = 4, Barcode = "BC020", Status = BookCopyStatus.Available, Condition = BookCondition.New, Location = "D5", CreatedAt = now }
            );

            // BORROW TRANSACTION
            modelBuilder.Entity<BorrowTransaction>().HasData(
                new BorrowTransaction { BorrowTransactionId = 1, UserId = 2, BorrowDate = now.AddDays(-10), DueDate = now.AddDays(-3), Status = "Returned", CreatedAt = now },
                new BorrowTransaction { BorrowTransactionId = 2, UserId = 3, BorrowDate = now.AddDays(-12), DueDate = now.AddDays(-5), Status = "Returned", CreatedAt = now },
                new BorrowTransaction { BorrowTransactionId = 3, UserId = 4, BorrowDate = now.AddDays(-2), DueDate = now.AddDays(5), Status = "Borrowing", CreatedAt = now },
                new BorrowTransaction { BorrowTransactionId = 4, UserId = 5, BorrowDate = now.AddDays(-15), DueDate = now.AddDays(-7), Status = "Overdue", CreatedAt = now },
                new BorrowTransaction { BorrowTransactionId = 5, UserId = 6, BorrowDate = now.AddDays(-20), DueDate = now.AddDays(-10), Status = "Lost", CreatedAt = now },
                new BorrowTransaction { BorrowTransactionId = 6, UserId = 7, BorrowDate = now.AddDays(-8), DueDate = now.AddDays(-2), Status = "Damaged", CreatedAt = now },
                new BorrowTransaction { BorrowTransactionId = 7, UserId = 8, BorrowDate = now.AddDays(-5), DueDate = now.AddDays(2), Status = "Borrowing", CreatedAt = now },
                new BorrowTransaction { BorrowTransactionId = 8, UserId = 9, BorrowDate = now.AddDays(-7), DueDate = now.AddDays(-1), Status = "Returned", CreatedAt = now },
                new BorrowTransaction { BorrowTransactionId = 9, UserId = 10, BorrowDate = now.AddDays(-3), DueDate = now.AddDays(4), Status = "Borrowing", CreatedAt = now },
                new BorrowTransaction { BorrowTransactionId = 10, UserId = 2, BorrowDate = now.AddDays(-30), DueDate = now.AddDays(-20), Status = "Returned", CreatedAt = now },
                new BorrowTransaction { BorrowTransactionId = 101, UserId = 4, BorrowDate = now.AddDays(-20), DueDate = now.AddDays(-10), Status = "Overdue", CreatedAt = now },
                new BorrowTransaction { BorrowTransactionId = 102, UserId = 8, BorrowDate = now.AddDays(-15), DueDate = now.AddDays(-5), Status = "Lost", CreatedAt = now },
                new BorrowTransaction { BorrowTransactionId = 103, UserId = 9, BorrowDate = now.AddDays(-12), DueDate = now.AddDays(-2), Status = "Damaged", CreatedAt = now }
            );

            // BORROW DETAIL
            modelBuilder.Entity<BorrowDetail>().HasData(
                new BorrowDetail { BorrowDetailId = 1, BorrowTransactionId = 1, BookCopyId = 1, BorrowDate = now.AddDays(-10), DueDate = now.AddDays(-3), ActualReturnDate = now.AddDays(-3), FineAmount = 0, CreatedAt = now },
                new BorrowDetail { BorrowDetailId = 2, BorrowTransactionId = 2, BookCopyId = 2, BorrowDate = now.AddDays(-12), DueDate = now.AddDays(-5), ActualReturnDate = now.AddDays(-2), FineAmount = 5000, CreatedAt = now },
                new BorrowDetail { BorrowDetailId = 3, BorrowTransactionId = 3, BookCopyId = 3, BorrowDate = now.AddDays(-2), DueDate = now.AddDays(5), ActualReturnDate = null, FineAmount = 0, CreatedAt = now },
                new BorrowDetail { BorrowDetailId = 4, BorrowTransactionId = 4, BookCopyId = 4, BorrowDate = now.AddDays(-15), DueDate = now.AddDays(-7), ActualReturnDate = null, FineAmount = 3000, CreatedAt = now },
                new BorrowDetail { BorrowDetailId = 5, BorrowTransactionId = 5, BookCopyId = 5, BorrowDate = now.AddDays(-20), DueDate = now.AddDays(-10), ActualReturnDate = null, FineAmount = 100000, CreatedAt = now },
                new BorrowDetail { BorrowDetailId = 6, BorrowTransactionId = 6, BookCopyId = 6, BorrowDate = now.AddDays(-8), DueDate = now.AddDays(-2), ActualReturnDate = now.AddDays(-1), FineAmount = 50000, CreatedAt = now },
                new BorrowDetail { BorrowDetailId = 7, BorrowTransactionId = 7, BookCopyId = 7, BorrowDate = now.AddDays(-5), DueDate = now.AddDays(2), ActualReturnDate = null, FineAmount = 0, CreatedAt = now },
                new BorrowDetail { BorrowDetailId = 8, BorrowTransactionId = 8, BookCopyId = 8, BorrowDate = now.AddDays(-7), DueDate = now.AddDays(-1), ActualReturnDate = now.AddDays(-1), FineAmount = 0, CreatedAt = now },
                new BorrowDetail { BorrowDetailId = 9, BorrowTransactionId = 9, BookCopyId = 9, BorrowDate = now.AddDays(-3), DueDate = now.AddDays(4), ActualReturnDate = null, FineAmount = 0, CreatedAt = now },
                new BorrowDetail { BorrowDetailId = 10, BorrowTransactionId = 10, BookCopyId = 10, BorrowDate = now.AddDays(-30), DueDate = now.AddDays(-20), ActualReturnDate = now.AddDays(-20), FineAmount = 0, CreatedAt = now },
                new BorrowDetail { BorrowDetailId = 101, BorrowTransactionId = 101, BookCopyId = 11, BorrowDate = now.AddDays(-20), DueDate = now.AddDays(-10), ActualReturnDate = null, FineAmount = 50000, CreatedAt = now },
                new BorrowDetail { BorrowDetailId = 102, BorrowTransactionId = 102, BookCopyId = 12, BorrowDate = now.AddDays(-15), DueDate = now.AddDays(-5), ActualReturnDate = null, FineAmount = 100000, CreatedAt = now },
                new BorrowDetail { BorrowDetailId = 103, BorrowTransactionId = 103, BookCopyId = 13, BorrowDate = now.AddDays(-12), DueDate = now.AddDays(-2), ActualReturnDate = null, FineAmount = 50000, CreatedAt = now }
            );

            // RESERVATION
            modelBuilder.Entity<Reservation>().HasData(
                new Reservation { ReservationId = 1, UserId = 2, BookId = 1, ReservedAt = now, Status = ReservationStatus.Pending, CreatedAt = now },
                new Reservation { ReservationId = 2, UserId = 3, BookId = 1, ReservedAt = now, Status = ReservationStatus.Allocated, CreatedAt = now },
                new Reservation { ReservationId = 3, UserId = 4, BookId = 1, ReservedAt = now, Status = ReservationStatus.Completed, CreatedAt = now },
                new Reservation { ReservationId = 4, UserId = 5, BookId = 1, ReservedAt = now, Status = ReservationStatus.Cancelled, CreatedAt = now },
                new Reservation { ReservationId = 5, UserId = 6, BookId = 2, ReservedAt = now, Status = ReservationStatus.Expired, CreatedAt = now },
                new Reservation { ReservationId = 6, UserId = 7, BookId = 2, ReservedAt = now, Status = ReservationStatus.Pending, CreatedAt = now },
                new Reservation { ReservationId = 7, UserId = 8, BookId = 2, ReservedAt = now, Status = ReservationStatus.Allocated, CreatedAt = now },
                new Reservation { ReservationId = 8, UserId = 9, BookId = 2, ReservedAt = now, Status = ReservationStatus.Completed, CreatedAt = now },
                new Reservation { ReservationId = 9, UserId = 10, BookId = 2, ReservedAt = now, Status = ReservationStatus.Cancelled, CreatedAt = now },
                new Reservation { ReservationId = 10, UserId = 2, BookId = 2, ReservedAt = now, Status = ReservationStatus.Pending, CreatedAt = now }
            );

            // PAYMENT
            modelBuilder.Entity<Payment>().HasData(
                new Payment { PaymentId = 1, UserId = 2, BorrowTransactionId = 1, Amount = 5000, PaymentMethod = PaymentMethod.Cash, PaymentStatus = PaymentStatus.Success, PaidAt = now, CreatedAt = now },
                new Payment { PaymentId = 2, UserId = 3, BorrowTransactionId = 1, Amount = 10000, PaymentMethod = PaymentMethod.Cash, PaymentStatus = PaymentStatus.Success, PaidAt = now, CreatedAt = now },
                new Payment { PaymentId = 3, UserId = 4, BorrowTransactionId = 1, Amount = 20000, PaymentMethod = PaymentMethod.VnPay, PaymentStatus = PaymentStatus.Pending, CreatedAt = now },
                new Payment { PaymentId = 4, UserId = 5, BorrowTransactionId = 1, Amount = 15000, PaymentMethod = PaymentMethod.Cash, PaymentStatus = PaymentStatus.Failed, CreatedAt = now },
                new Payment { PaymentId = 5, UserId = 6, BorrowTransactionId = 1, Amount = 7000, PaymentMethod = PaymentMethod.Cash, PaymentStatus = PaymentStatus.Success, PaidAt = now, CreatedAt = now },
                new Payment { PaymentId = 6, UserId = 7, BorrowTransactionId = 1, Amount = 9000, PaymentMethod = PaymentMethod.Cash, PaymentStatus = PaymentStatus.Refunded, CreatedAt = now },
                new Payment { PaymentId = 7, UserId = 8, BorrowTransactionId = 1, Amount = 3000, PaymentMethod = PaymentMethod.VnPay, PaymentStatus = PaymentStatus.Success, PaidAt = now, CreatedAt = now },
                new Payment { PaymentId = 8, UserId = 9, BorrowTransactionId = 1, Amount = 4500, PaymentMethod = PaymentMethod.Cash, PaymentStatus = PaymentStatus.Pending, CreatedAt = now },
                new Payment { PaymentId = 9, UserId = 10, BorrowTransactionId = 1, Amount = 8000, PaymentMethod = PaymentMethod.Cash, PaymentStatus = PaymentStatus.Success, PaidAt = now, CreatedAt = now },
                new Payment { PaymentId = 10, UserId = 2, BorrowTransactionId = 1, Amount = 12000, PaymentMethod = PaymentMethod.Cash, PaymentStatus = PaymentStatus.Success, PaidAt = now, CreatedAt = now },
                new Payment { PaymentId = 11, UserId = 3, BorrowTransactionId = 2, Amount = 5000, PaymentMethod = PaymentMethod.Cash, PaymentStatus = PaymentStatus.Success, PaidAt = now, CreatedAt = now },
                new Payment { PaymentId = 12, UserId = 6, BorrowTransactionId = 5, Amount = 100000, PaymentMethod = PaymentMethod.Cash, PaymentStatus = PaymentStatus.Success, PaidAt = now, CreatedAt = now },
                new Payment { PaymentId = 13, UserId = 7, BorrowTransactionId = 6, Amount = 50000, PaymentMethod = PaymentMethod.Cash, PaymentStatus = PaymentStatus.Pending, CreatedAt = now }
            );

            // BOOK REVIEW
            modelBuilder.Entity<BookReview>().HasData(
                new BookReview { BookReviewId = 3, UserId = 2, BookId = 1, Rating = 5, Comment = "Rất hay", CreatedAt = now },
                new BookReview { BookReviewId = 4, UserId = 3, BookId = 1, Rating = 4, Comment = "Đọc lại vẫn xúc động", CreatedAt = now },
                new BookReview { BookReviewId = 5, UserId = 4, BookId = 1, Rating = 3, Comment = "Khá ổn", CreatedAt = now },
                new BookReview { BookReviewId = 6, UserId = 5, BookId = 1, Rating = 5, Comment = "Tuổi thơ ùa về", CreatedAt = now },
                new BookReview { BookReviewId = 7, UserId = 6, BookId = 2, Rating = 4, Comment = "Tái bản đẹp", CreatedAt = now },
                new BookReview { BookReviewId = 8, UserId = 7, BookId = 2, Rating = 2, Comment = "Nội dung hơi chậm", CreatedAt = now },
                new BookReview { BookReviewId = 9, UserId = 8, BookId = 2, Rating = 5, Comment = "Tuyệt vời", CreatedAt = now },
                new BookReview { BookReviewId = 10, UserId = 9, BookId = 2, Rating = 4, Comment = "Đáng đọc", CreatedAt = now },
                new BookReview { BookReviewId = 11, UserId = 10, BookId = 1, Rating = 5, Comment = "Xuất sắc", CreatedAt = now },
                new BookReview { BookReviewId = 12, UserId = 2, BookId = 2, Rating = 4, Comment = "Phiên bản mới đẹp hơn", CreatedAt = now }
            );
        }
    }
}
