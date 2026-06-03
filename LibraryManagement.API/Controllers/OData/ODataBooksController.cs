using LibraryManagement.BLL.Services;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace LibraryManagement.API.Controllers.OData
{
    [ApiController]
    [Route("odata")]
    public class ODataBooksController : ControllerBase
    {
        private readonly BookCatalogService bookCatalogService;
        private readonly InventoryService inventoryService;

        public ODataBooksController(BookCatalogService _bookCatalogService, InventoryService _inventoryService)
        {
            bookCatalogService = _bookCatalogService;
            inventoryService = _inventoryService;
        }

        [HttpGet("Books")]
        [EnableQuery(MaxTop = 100)]
        public IQueryable<Book> GetBooks()
        {
            return bookCatalogService.QueryBooks();
        }

        [HttpGet("BookCopies")]
        [EnableQuery(MaxTop = 100)]
        public IQueryable<BookCopy> GetBookCopies()
        {
            return inventoryService.QueryCopies();
        }

        [HttpGet("Authors")]
        [EnableQuery(MaxTop = 100)]
        public IQueryable<Author> GetAuthors()
        {
            return bookCatalogService.QueryAuthors();
        }

        [HttpGet("Categories")]
        [EnableQuery(MaxTop = 100)]
        public IQueryable<Category> GetCategories()
        {
            return bookCatalogService.QueryCategories();
        }

        [HttpGet("Publishers")]
        [EnableQuery(MaxTop = 100)]
        public IQueryable<Publisher> GetPublishers()
        {
            return bookCatalogService.QueryPublishers();
        }
    }
}
