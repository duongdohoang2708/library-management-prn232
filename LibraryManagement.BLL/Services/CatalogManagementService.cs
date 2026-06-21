using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.DTO.Catalog;
using LibraryManagementDAL.Models;

namespace LibraryManagement.BLL.Services
{
    public class CatalogManagementService
    {
        private readonly CatalogManagementRepository catalogRepository;
        private readonly AuditLogService auditLogService;

        public CatalogManagementService(
            CatalogManagementRepository _catalogRepository,
            AuditLogService _auditLogService)
        {
            catalogRepository = _catalogRepository;
            auditLogService = _auditLogService;
        }

        public async Task<CatalogIndexResult> GetAllAsync()
        {
            return new CatalogIndexResult
            {
                Authors = await catalogRepository.GetAuthorsAsync(),
                Categories = await catalogRepository.GetCategoriesAsync(),
                Publishers = await catalogRepository.GetPublishersAsync()
            };
        }

        public async Task<CatalogActionResponse> CreateAuthorAsync(AuthorSaveRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Fail("Author name is required.");
            }

            if (await catalogRepository.AuthorNameExistsAsync(request.Name))
            {
                return Fail("Author name already exists.");
            }

            var author = new Author
            {
                Name = request.Name.Trim(),
                Biography = request.Biography?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            catalogRepository.AddAuthor(author);
            await catalogRepository.SaveChangesAsync();
            await auditLogService.LogAsync("CreateAuthor", "Author", author.AuthorId.ToString(), $"Author \"{author.Name}\" created.");

            return Ok("Author created.", author.AuthorId);
        }

        public async Task<CatalogActionResponse> UpdateAuthorAsync(int id, AuthorSaveRequest request)
        {
            var author = await catalogRepository.GetAuthorAsync(id);
            if (author == null)
            {
                return Fail("Author not found.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Fail("Author name is required.");
            }

            if (await catalogRepository.AuthorNameExistsAsync(request.Name, id))
            {
                return Fail("Author name already exists.");
            }

            author.Name = request.Name.Trim();
            author.Biography = request.Biography?.Trim();
            author.UpdatedAt = DateTime.UtcNow;

            await catalogRepository.SaveChangesAsync();
            await auditLogService.LogAsync("UpdateAuthor", "Author", author.AuthorId.ToString(), $"Author \"{author.Name}\" updated.");

            return Ok("Author updated.", author.AuthorId);
        }

        public async Task<CatalogActionResponse> DeleteAuthorAsync(int id)
        {
            var author = await catalogRepository.GetAuthorAsync(id);
            if (author == null)
            {
                return Fail("Author not found.");
            }

            if (await catalogRepository.AuthorHasBooksAsync(id))
            {
                return Fail("Cannot delete this author because books are using it.");
            }

            catalogRepository.RemoveAuthor(author);
            await catalogRepository.SaveChangesAsync();
            await auditLogService.LogAsync("DeleteAuthor", "Author", id.ToString(), $"Author \"{author.Name}\" deleted.");

            return Ok("Author deleted.", id);
        }

        public async Task<CatalogActionResponse> CreateCategoryAsync(CategorySaveRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CategoryName))
            {
                return Fail("Category name is required.");
            }

            if (await catalogRepository.CategoryNameExistsAsync(request.CategoryName))
            {
                return Fail("Category name already exists.");
            }

            var category = new Category
            {
                CategoryName = request.CategoryName.Trim(),
                Description = request.Description?.Trim() ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            catalogRepository.AddCategory(category);
            await catalogRepository.SaveChangesAsync();
            await auditLogService.LogAsync("CreateCategory", "Category", category.CategoryId.ToString(), $"Category \"{category.CategoryName}\" created.");

            return Ok("Category created.", category.CategoryId);
        }

        public async Task<CatalogActionResponse> UpdateCategoryAsync(int id, CategorySaveRequest request)
        {
            var category = await catalogRepository.GetCategoryAsync(id);
            if (category == null)
            {
                return Fail("Category not found.");
            }

            if (string.IsNullOrWhiteSpace(request.CategoryName))
            {
                return Fail("Category name is required.");
            }

            if (await catalogRepository.CategoryNameExistsAsync(request.CategoryName, id))
            {
                return Fail("Category name already exists.");
            }

            category.CategoryName = request.CategoryName.Trim();
            category.Description = request.Description?.Trim() ?? string.Empty;
            category.UpdatedAt = DateTime.UtcNow;

            await catalogRepository.SaveChangesAsync();
            await auditLogService.LogAsync("UpdateCategory", "Category", category.CategoryId.ToString(), $"Category \"{category.CategoryName}\" updated.");

            return Ok("Category updated.", category.CategoryId);
        }

        public async Task<CatalogActionResponse> DeleteCategoryAsync(int id)
        {
            var category = await catalogRepository.GetCategoryAsync(id);
            if (category == null)
            {
                return Fail("Category not found.");
            }

            if (await catalogRepository.CategoryHasBooksAsync(id))
            {
                return Fail("Cannot delete this category because books are using it.");
            }

            catalogRepository.RemoveCategory(category);
            await catalogRepository.SaveChangesAsync();
            await auditLogService.LogAsync("DeleteCategory", "Category", id.ToString(), $"Category \"{category.CategoryName}\" deleted.");

            return Ok("Category deleted.", id);
        }

        public async Task<CatalogActionResponse> CreatePublisherAsync(PublisherSaveRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PublisherName))
            {
                return Fail("Publisher name is required.");
            }

            if (await catalogRepository.PublisherNameExistsAsync(request.PublisherName))
            {
                return Fail("Publisher name already exists.");
            }

            var publisher = new Publisher
            {
                PublisherName = request.PublisherName.Trim(),
                Address = request.Address?.Trim() ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            catalogRepository.AddPublisher(publisher);
            await catalogRepository.SaveChangesAsync();
            await auditLogService.LogAsync("CreatePublisher", "Publisher", publisher.PublisherId.ToString(), $"Publisher \"{publisher.PublisherName}\" created.");

            return Ok("Publisher created.", publisher.PublisherId);
        }

        public async Task<CatalogActionResponse> UpdatePublisherAsync(int id, PublisherSaveRequest request)
        {
            var publisher = await catalogRepository.GetPublisherAsync(id);
            if (publisher == null)
            {
                return Fail("Publisher not found.");
            }

            if (string.IsNullOrWhiteSpace(request.PublisherName))
            {
                return Fail("Publisher name is required.");
            }

            if (await catalogRepository.PublisherNameExistsAsync(request.PublisherName, id))
            {
                return Fail("Publisher name already exists.");
            }

            publisher.PublisherName = request.PublisherName.Trim();
            publisher.Address = request.Address?.Trim() ?? string.Empty;
            publisher.UpdatedAt = DateTime.UtcNow;

            await catalogRepository.SaveChangesAsync();
            await auditLogService.LogAsync("UpdatePublisher", "Publisher", publisher.PublisherId.ToString(), $"Publisher \"{publisher.PublisherName}\" updated.");

            return Ok("Publisher updated.", publisher.PublisherId);
        }

        public async Task<CatalogActionResponse> DeletePublisherAsync(int id)
        {
            var publisher = await catalogRepository.GetPublisherAsync(id);
            if (publisher == null)
            {
                return Fail("Publisher not found.");
            }

            if (await catalogRepository.PublisherHasBooksAsync(id))
            {
                return Fail("Cannot delete this publisher because books are using it.");
            }

            catalogRepository.RemovePublisher(publisher);
            await catalogRepository.SaveChangesAsync();
            await auditLogService.LogAsync("DeletePublisher", "Publisher", id.ToString(), $"Publisher \"{publisher.PublisherName}\" deleted.");

            return Ok("Publisher deleted.", id);
        }

        private static CatalogActionResponse Ok(string message, int id)
        {
            return new CatalogActionResponse
            {
                IsSuccess = true,
                Message = message,
                Id = id
            };
        }

        private static CatalogActionResponse Fail(string message)
        {
            return new CatalogActionResponse
            {
                IsSuccess = false,
                Message = message
            };
        }
    }
}
