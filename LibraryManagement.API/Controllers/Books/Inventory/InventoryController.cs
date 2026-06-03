    using LibraryManagement.BLL.DTO.Inventory;
using LibraryManagement.BLL.Services;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Books.Inventory
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryService inventoryService;

        public InventoryController(InventoryService _inventoryService)
        {
            inventoryService = _inventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCopies(string? searchBarcode, BookCopyStatus? status, BookCondition? condition, string? location, int page = 1)
        {
            return Ok(await inventoryService.GetCopiesAsync(searchBarcode, status, condition, location, page));
        }

        [HttpGet("book/{bookId:int}")]
        public async Task<IActionResult> GetBookCopies(int bookId)
        {
            var result = await inventoryService.GetBookCopiesAsync(bookId);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost("book/{bookId:int}/copies")]
        public async Task<IActionResult> AddCopies(int bookId, AddCopiesRequest request)
        {
            var numberOfCopies = await inventoryService.AddCopiesAsync(bookId, request);
            if (!numberOfCopies.HasValue)
            {
                return NotFound();
            }

            return Ok(new { bookId, numberOfCopies });
        }

        [HttpPut("copies/{copyId:int}")]
        public async Task<IActionResult> UpdateCopy(int copyId, UpdateCopyRequest request)
        {
            var isUpdated = await inventoryService.UpdateCopyAsync(copyId, request);
            if (!isUpdated)
            {
                return NotFound();
            }

            return Ok(new { copyId, request.BookId });
        }
    }
}
