using LibraryManagement.BLL.DTO.Inventory;
using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.BLL.Services
{
    public class InventoryService
    {
        private const int PageSize = 10;
        private readonly InventoryRepository inventoryRepository;

        public InventoryService(InventoryRepository _inventoryRepository)
        {
            inventoryRepository = _inventoryRepository;
        }

        public async Task<InventoryListResult> GetCopiesAsync(
            string? searchBarcode,
            BookCopyStatus? status,
            BookCondition? condition,
            string? location,
            int page)
        {
            page = page < 1 ? 1 : page;
            var query = inventoryRepository.QueryCopies();

            if (!string.IsNullOrWhiteSpace(searchBarcode))
            {
                var key = searchBarcode.Trim();
                query = query.Where(c => c.Barcode.Contains(key) || c.Book.Title.Contains(key));
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            if (condition.HasValue)
            {
                query = query.Where(c => c.Condition == condition.Value);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                var key = location.Trim();
                query = query.Where(c => c.Location != null && c.Location.Contains(key));
            }

            query = query.OrderBy(c => c.Book.Title).ThenBy(c => c.Barcode);

            var totalItems = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
            page = Math.Min(page, totalPages);

            return new InventoryListResult
            {
                Items = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(),
                TotalPages = totalPages,
                CurrentPage = page
            };
        }

        public async Task<BookCopiesResult?> GetBookCopiesAsync(int bookId)
        {
            var book = await inventoryRepository.GetBookByIdAsync(bookId);
            if (book == null)
            {
                return null;
            }

            return new BookCopiesResult
            {
                BookId = book.BookId,
                BookTitle = book.Title,
                Items = await inventoryRepository.GetCopiesByBookIdAsync(bookId)
            };
        }

        public IQueryable<BookCopy> QueryCopies()
        {
            return inventoryRepository.QueryCopies();
        }

        public async Task<int?> AddCopiesAsync(int bookId, AddCopiesRequest request)
        {
            var book = await inventoryRepository.GetBookByIdAsync(bookId);
            if (book == null)
            {
                return null;
            }

            var numberOfCopies = Math.Clamp(request.NumberOfCopies, 1, 50);
            var nextNumber = await inventoryRepository.GetNextCopyNumberAsync();

            for (var i = 0; i < numberOfCopies; i++)
            {
                inventoryRepository.AddCopy(new BookCopy
                {
                    BookId = bookId,
                    Barcode = $"BC{nextNumber + i:00000}",
                    Status = BookCopyStatus.Available,
                    Condition = BookCondition.New,
                    Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
                    CreatedAt = DateTime.UtcNow
                });
            }

            await inventoryRepository.SaveChangesAsync();
            return numberOfCopies;
        }

        public async Task<bool> UpdateCopyAsync(int copyId, UpdateCopyRequest request)
        {
            var copy = await inventoryRepository.GetCopyByIdAsync(copyId);
            if (copy == null)
            {
                return false;
            }

            if (copy.Status == BookCopyStatus.Borrowed || copy.Status == BookCopyStatus.Reserved)
            {
                request.Status = copy.Status;
            }

            copy.Status = request.Status;
            copy.Condition = request.Condition;
            copy.Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();
            copy.UpdatedAt = DateTime.UtcNow;

            await inventoryRepository.SaveChangesAsync();
            return true;
        }
    }
}
