using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
namespace LibraryManagement.DAL.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        #region DbSets

        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Member> Members => Set<Member>();
        public DbSet<Staff> Staffs => Set<Staff>();
        public DbSet<Role> Roles => Set<Role>();

        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Publisher> Publishers => Set<Publisher>();

        public DbSet<Book> Books => Set<Book>();
        public DbSet<BookCopy> BookCopies => Set<BookCopy>();

        public DbSet<BorrowTransaction> BorrowTransactions => Set<BorrowTransaction>();
        public DbSet<BorrowDetail> BorrowDetails => Set<BorrowDetail>();

        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<PaymentDetail> PaymentDetails => Set<PaymentDetail>();
        public DbSet<BookReview> BookReviews => Set<BookReview>();
        public DbSet<BookAISummary> BookAISummary { get; set; }
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
        public DbSet<ReminderLog> ReminderLogs => Set<ReminderLog>();

        public DbSet<AIRequestLog> AIRequestLogs => Set<AIRequestLog>();
        public DbSet<AIRequestLogDetail> AIRequestLogDetails => Set<AIRequestLogDetail>();
        public DbSet<Notification> Notification => Set<Notification>();

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureEnums(modelBuilder);
            ConfigureDecimals(modelBuilder);
            ConfigureRelationships(modelBuilder);
            ConfigureIndexes(modelBuilder);
            ApplySoftDeleteFilter(modelBuilder);

            LibraryManagementDAL.Data.SeedData.Seed(modelBuilder);

            modelBuilder.Entity<AIRequestLog>()
                .HasOne(x => x.Detail)
                .WithOne(x => x.AIRequestLog)
                .HasForeignKey<AIRequestLogDetail>(x => x.AIRequestLogId);

            modelBuilder.Entity<BookAISummary>()
                .HasOne(x => x.Book)
                .WithOne(x => x.BookAISummary)
                .HasForeignKey<BookAISummary>(x => x.BookId);
            modelBuilder.Entity<BookAISummary>()
                .HasIndex(x => x.BookId)
                .IsUnique();

            modelBuilder.Entity<SystemSetting>()
                .HasIndex(x => x.Key)
                .IsUnique();

            modelBuilder.Entity<ReminderLog>()
                .HasIndex(x => new { x.BorrowDetailId, x.ReminderType, x.ReminderDate })
                .IsUnique();
        }

        // ================= ENUM =================
        private void ConfigureEnums(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BookCopy>().Property(x => x.Status).HasConversion<int>();
            modelBuilder.Entity<BookCopy>().Property(x => x.Condition).HasConversion<int>();
            modelBuilder.Entity<Reservation>().Property(x => x.Status).HasConversion<int>();
            modelBuilder.Entity<Payment>().Property(x => x.PaymentMethod).HasConversion<int>();
            modelBuilder.Entity<Payment>().Property(x => x.PaymentStatus).HasConversion<int>();
        }

        // ================= DECIMAL =================
        private void ConfigureDecimals(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BorrowDetail>()
                .Property(x => x.FineAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BorrowDetail>()
                .Property(x => x.FinePaidAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PaymentDetail>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);
        }

        // ================= RELATIONSHIP =================
        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>()
                .ToTable("Accounts");

            modelBuilder.Entity<BorrowTransaction>()
                .Ignore(x => x.User);

            modelBuilder.Entity<Payment>()
                .Ignore(x => x.User);

            modelBuilder.Entity<Notification>()
                .Ignore(x => x.User);

            modelBuilder.Entity<Reservation>()
                .Ignore(x => x.User);

            modelBuilder.Entity<BookReview>()
                .Ignore(x => x.User);

            modelBuilder.Entity<Member>()
                .HasOne(x => x.Account)
                .WithOne(x => x.Member)
                .HasForeignKey<Member>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Staff>()
                .HasOne(x => x.Account)
                .WithOne(x => x.Staff)
                .HasForeignKey<Staff>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Staff>()
                .HasOne(x => x.Role)
                .WithMany(x => x.Staffs)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // BOOK RELATION (NO CASCADE DANGEROUS)
            modelBuilder.Entity<Book>()
                .HasOne(x => x.Author)
                .WithMany(x => x.Books)
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Book>()
                .HasOne(x => x.Category)
                .WithMany(x => x.Books)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Book>()
                .HasOne(x => x.Publisher)
                .WithMany(x => x.Books)
                .HasForeignKey(x => x.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);

            // PAYMENT
            modelBuilder.Entity<BorrowTransaction>()
                .HasOne(x => x.Account)
                .WithMany(x => x.BorrowTransactions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookReview>()
                .HasOne(x => x.Account)
                .WithMany(x => x.BookReviews)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(x => x.Account)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reservation>()
                .HasOne(x => x.Account)
                .WithMany(x => x.Reservations)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .HasOne(x => x.Account)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment>()
                .HasOne(x => x.BorrowTransaction)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.BorrowTransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PaymentDetail>()
                .HasOne(x => x.Payment)
                .WithMany(x => x.PaymentDetails)
                .HasForeignKey(x => x.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PaymentDetail>()
                .HasOne(x => x.BorrowDetail)
                .WithMany()
                .HasForeignKey(x => x.BorrowDetailId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ReminderLog>()
                .HasOne(x => x.BorrowDetail)
                .WithMany()
                .HasForeignKey(x => x.BorrowDetailId)
                .OnDelete(DeleteBehavior.Cascade);

            // AI LOG 1-1
            modelBuilder.Entity<AIRequestLog>()
                .HasOne(x => x.Detail)
                .WithOne(x => x.AIRequestLog)
                .HasForeignKey<AIRequestLogDetail>(x => x.AIRequestLogId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        // ================= INDEX =================
        private void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>().HasIndex(x => x.ISBN).IsUnique();
            modelBuilder.Entity<Account>().HasIndex(x => x.Email).IsUnique();
            modelBuilder.Entity<Member>().HasIndex(x => x.UserId).IsUnique();
            modelBuilder.Entity<Member>().HasIndex(x => x.MemberCode).IsUnique();
            modelBuilder.Entity<Staff>().HasIndex(x => x.UserId).IsUnique();
            modelBuilder.Entity<Staff>().HasIndex(x => x.StaffCode).IsUnique();
            modelBuilder.Entity<BookCopy>().HasIndex(x => x.Barcode).IsUnique();

            modelBuilder.Entity<Reservation>()
                .HasIndex(x => new { x.BookId, x.Status });

            modelBuilder.Entity<BorrowDetail>()
                .HasIndex(x => x.BookCopyId);
        }

        // ================= SOFT DELETE GLOBAL =================
        private void ApplySoftDeleteFilter(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var prop = Expression.Property(parameter, "IsDeleted");
                    var condition = Expression.Equal(prop, Expression.Constant(false));
                    var lambda = Expression.Lambda(condition, parameter);

                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(lambda);
                }
            }
        }

        // ================= AUTO SOFT DELETE =================
        public override int SaveChanges()
        {
            SoftDelete();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SoftDelete();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SoftDelete()
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>()
                         .Where(x => x.State == EntityState.Deleted))
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }
        }
    }
}
