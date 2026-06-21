using LibraryManagement.BLL.DTO.Books;
using LibraryManagement.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Books
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly BookCatalogService bookCatalogService;

        public BooksController(BookCatalogService _bookCatalogService)
        {
            bookCatalogService = _bookCatalogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks(
            string? keyword,
            string? category,
            int? publisherId,
            int? publishYear,
            bool? isActive,
            string? availability,
            int? minRating,
            string? sort,
            int page = 1,
            int pageSize = 12)
        {
            return Ok(await bookCatalogService.GetBooksAsync(keyword, category, publisherId, publishYear, isActive, availability, minRating, sort, page, pageSize));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await bookCatalogService.GetBookDetailsAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        [HttpGet("options")]
        public async Task<IActionResult> GetOptions()
        {
            return Ok(await bookCatalogService.GetOptionsAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create(BookCreateRequest request)
        {
            var bookId = await bookCatalogService.CreateAsync(request);
            if (!bookId.HasValue)
            {
                return BadRequest(new { message = "Please select author, category, and publisher." });
            }

            return Ok(new { bookId });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, BookUpdateRequest request)
        {
            var bookId = await bookCatalogService.UpdateAsync(id, request);
            if (!bookId.HasValue)
            {
                return NotFound();
            }

            return Ok(new { bookId });
        }

        [HttpPost("{id:int}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await bookCatalogService.ToggleStatusAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(new { bookId = result.Value.BookId, isActive = result.Value.IsActive });
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { importedCount = 0, skippedCount = 0, errors = new[] { "Please choose an Excel file." } });
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { importedCount = 0, skippedCount = 0, errors = new[] { "Only .xlsx files are supported." } });
            }

            await using var stream = file.OpenReadStream();
            var result = await bookCatalogService.ImportAsync(stream);
            return Ok(result);
        }
    }
}
