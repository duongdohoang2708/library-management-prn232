using LibraryManagement.BLL.Services;
using LibraryManagementDAL.DTO.Catalog;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Catalog
{
    [ApiController]
    [Route("api/catalog")]
    public class CatalogController : ControllerBase
    {
        private readonly CatalogManagementService catalogService;

        public CatalogController(CatalogManagementService _catalogService)
        {
            catalogService = _catalogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await catalogService.GetAllAsync());
        }

        [HttpPost("authors")]
        public async Task<IActionResult> CreateAuthor(AuthorSaveRequest request)
        {
            return ToActionResult(await catalogService.CreateAuthorAsync(request));
        }

        [HttpPut("authors/{id:int}")]
        public async Task<IActionResult> UpdateAuthor(int id, AuthorSaveRequest request)
        {
            return ToActionResult(await catalogService.UpdateAuthorAsync(id, request));
        }

        [HttpDelete("authors/{id:int}")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            return ToActionResult(await catalogService.DeleteAuthorAsync(id));
        }

        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory(CategorySaveRequest request)
        {
            return ToActionResult(await catalogService.CreateCategoryAsync(request));
        }

        [HttpPut("categories/{id:int}")]
        public async Task<IActionResult> UpdateCategory(int id, CategorySaveRequest request)
        {
            return ToActionResult(await catalogService.UpdateCategoryAsync(id, request));
        }

        [HttpDelete("categories/{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            return ToActionResult(await catalogService.DeleteCategoryAsync(id));
        }

        [HttpPost("publishers")]
        public async Task<IActionResult> CreatePublisher(PublisherSaveRequest request)
        {
            return ToActionResult(await catalogService.CreatePublisherAsync(request));
        }

        [HttpPut("publishers/{id:int}")]
        public async Task<IActionResult> UpdatePublisher(int id, PublisherSaveRequest request)
        {
            return ToActionResult(await catalogService.UpdatePublisherAsync(id, request));
        }

        [HttpDelete("publishers/{id:int}")]
        public async Task<IActionResult> DeletePublisher(int id)
        {
            return ToActionResult(await catalogService.DeletePublisherAsync(id));
        }

        private IActionResult ToActionResult(CatalogActionResponse result)
        {
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
