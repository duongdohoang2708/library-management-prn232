using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Client.Controllers.Inventory
{
    public class InventoryController : Controller
    {
        private const int PageSize = 10;
        private readonly InventoryRepository inventoryRepository;

        public InventoryController(InventoryRepository _inventoryRepository)
        {
            inventoryRepository = _inventoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchBarcode, BookCopyStatus? status, BookCondition? condition, string? location, int page = 1)
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

            var copies = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.SearchBarcode = searchBarcode;
            ViewBag.Status = status;
            ViewBag.Condition = condition;
            ViewBag.Location = location;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_InventoryTable", copies);
            }

            return View(copies);
        }

        [HttpGet]
        public async Task<IActionResult> ManageCopies(int bookId)
        {
            var book = await inventoryRepository.GetBookByIdAsync(bookId);
            if (book == null)
            {
                return NotFound();
            }

            var copies = await inventoryRepository.GetCopiesByBookIdAsync(bookId);

            ViewBag.BookId = book.BookId;
            ViewBag.BookTitle = book.Title;
            return View(copies);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCopy(int bookId, int numberOfCopies, string? location)
        {
            var book = await inventoryRepository.GetBookByIdAsync(bookId);
            if (book == null)
            {
                return NotFound();
            }

            numberOfCopies = Math.Clamp(numberOfCopies, 1, 50);
            var nextNumber = await inventoryRepository.GetNextCopyNumberAsync();

            for (var i = 0; i < numberOfCopies; i++)
            {
                inventoryRepository.AddCopy(new BookCopy
                {
                    BookId = bookId,
                    Barcode = $"BC{nextNumber + i:00000}",
                    Status = BookCopyStatus.Available,
                    Condition = BookCondition.New,
                    Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim(),
                    CreatedAt = DateTime.UtcNow
                });
            }

            await inventoryRepository.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Added {numberOfCopies} copies.";
            return RedirectToAction(nameof(ManageCopies), new { bookId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int copyId, int bookId, BookCopyStatus status, BookCondition condition, string? location)
        {
            var copy = await inventoryRepository.GetCopyByIdAsync(copyId);
            if (copy == null)
            {
                return NotFound();
            }

            if (copy.Status == BookCopyStatus.Borrowed || copy.Status == BookCopyStatus.Reserved)
            {
                status = copy.Status;
            }

            copy.Status = status;
            copy.Condition = condition;
            copy.Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
            copy.UpdatedAt = DateTime.UtcNow;

            await inventoryRepository.SaveChangesAsync();
            TempData["SuccessMessage"] = "Copy updated.";
            return RedirectToAction(nameof(ManageCopies), new { bookId });
        }
    }
}
