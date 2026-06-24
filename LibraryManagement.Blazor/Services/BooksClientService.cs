using System.Net.Http.Json;
using LibraryManagement.Blazor.DTO.Books;
using LibraryManagement.Blazor.Models;
using LibraryManagementDAL.DTO.Book;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Components.Forms;
using OfficeOpenXml;

namespace LibraryManagement.Blazor.Services;

public sealed class BooksClientService : ApiClientBase
{
    public BooksClientService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, configuration, httpContextAccessor)
    {
    }

    public async Task<BookListApiResult> GetBookListAsync(BookListQuery query, CancellationToken cancellationToken = default)
    {
        var parts = new List<string>
        {
            $"page={Math.Max(1, query.Page)}",
            $"pageSize={Math.Max(1, query.PageSize)}"
        };

        AddQuery(parts, "keyword", query.Keyword);
        AddQuery(parts, "category", query.Category);
        AddQuery(parts, "publisherId", query.PublisherId?.ToString());
        AddQuery(parts, "publishYear", query.PublishYear?.ToString());
        AddQuery(parts, "isActive", query.IsActive?.ToString().ToLowerInvariant());
        AddQuery(parts, "availability", query.Availability);
        AddQuery(parts, "minRating", query.MinRating?.ToString());
        AddQuery(parts, "sort", query.Sort);

        return await GetAsync<BookListApiResult>($"api/books?{string.Join("&", parts)}", cancellationToken)
            ?? new BookListApiResult();
    }

    public async Task<Book?> GetBookAsync(int id, CancellationToken cancellationToken = default)
    {
        return await GetAsync<Book>($"api/books/{id}", cancellationToken);
    }

    public async Task<BookOptionsApiResult> GetBookOptionsAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<BookOptionsApiResult>("api/books/options", cancellationToken)
            ?? new BookOptionsApiResult();
    }

    public async Task<string?> GetAiSummaryAsync(int bookId, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeBearerToken: true);
        var response = await client.GetAsync(BuildApiUrl($"api/ai/books/{bookId}/summary"), cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<AiSummaryResponse>(cancellationToken);
        return payload?.SummaryText;
    }

    public async Task<OperationResult<int>> CreateBookAsync(BookMutationRequest request, CancellationToken cancellationToken = default)
    {
        request.Model.ImageUrl = await SaveCoverImageAsync(request.ImageFile, cancellationToken) ?? string.Empty;

        using var client = CreateClient(includeActorHeaders: true);
        var response = await client.PostAsJsonAsync(BuildApiUrl("api/books"), request.Model, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return OperationResult<int>.Failure("Create book failed.");
        }

        var result = await response.Content.ReadFromJsonAsync<BookSaveResult>(cancellationToken);
        return OperationResult<int>.Success(result?.BookId ?? 0, "Book created successfully.");
    }

    public async Task<OperationResult> UpdateBookAsync(int id, BookUpdateMutationRequest request, CancellationToken cancellationToken = default)
    {
        var imageUrl = await SaveCoverImageAsync(request.ImageFile, cancellationToken);
        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            request.Model.ImageUrl = imageUrl;
        }

        using var client = CreateClient(includeActorHeaders: true);
        var response = await client.PutAsJsonAsync(BuildApiUrl($"api/books/{id}"), request.Model, cancellationToken);
        return response.IsSuccessStatusCode
            ? OperationResult.Success("Book updated successfully.")
            : OperationResult.Failure("Update book failed.");
    }

    public async Task<OperationResult> ToggleStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeActorHeaders: true);
        var response = await client.PostAsync(BuildApiUrl($"api/books/{id}/toggle-status"), null, cancellationToken);
        return response.IsSuccessStatusCode
            ? OperationResult.Success("Book status updated.")
            : OperationResult.Failure("Could not update book status.");
    }

    public byte[] BuildImportTemplate()
    {
        ExcelPackage.License.SetNonCommercialPersonal("LMS Standard");
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Books");
        var headers = new[]
        {
            "Title",
            "ISBN",
            "Description",
            "PublishYear",
            "EditionNumber",
            "AuthorName",
            "CategoryName",
            "PublisherName",
            "PublisherAddress",
            "ImageUrl",
            "IsActive"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            sheet.Cells[1, i + 1].Value = headers[i];
            sheet.Cells[1, i + 1].Style.Font.Bold = true;
        }

        sheet.Cells[2, 1].Value = "Example Book";
        sheet.Cells[2, 2].Value = "9780000000000";
        sheet.Cells[2, 3].Value = "Short description";
        sheet.Cells[2, 4].Value = DateTime.UtcNow.Year;
        sheet.Cells[2, 5].Value = 1;
        sheet.Cells[2, 6].Value = "Author Name";
        sheet.Cells[2, 7].Value = "Category Name";
        sheet.Cells[2, 8].Value = "Publisher Name";
        sheet.Cells[2, 9].Value = "Publisher Address";
        sheet.Cells[2, 10].Value = "/uploads/books/example.jpg";
        sheet.Cells[2, 11].Value = "true";
        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

        return package.GetAsByteArray();
    }

    public async Task<OperationResult<BookImportApiResult>> ImportAsync(IBrowserFile file, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeActorHeaders: true);
        using var form = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream(20 * 1024 * 1024, cancellationToken);
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        form.Add(fileContent, "file", file.Name);

        var response = await client.PostAsync(BuildApiUrl("api/books/import"), form, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<BookImportApiResult>(cancellationToken) ?? new BookImportApiResult();

        return response.IsSuccessStatusCode
            ? OperationResult<BookImportApiResult>.Success(result, $"Imported {result.ImportedCount} books. Skipped {result.SkippedCount} rows.")
            : OperationResult<BookImportApiResult>.Failure(result.Errors.FirstOrDefault() ?? "Import books failed.", result.Errors);
    }

    private async Task<string?> SaveCoverImageAsync(IBrowserFile? imageFile, CancellationToken cancellationToken)
    {
        if (imageFile is null || imageFile.Size == 0)
        {
            return null;
        }

        var extension = Path.GetExtension(imageFile.Name);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var relativeFolder = Path.Combine("uploads", "books");
        var absoluteFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativeFolder);
        Directory.CreateDirectory(absoluteFolder);

        var absolutePath = Path.Combine(absoluteFolder, fileName);
        await using var destination = File.Create(absolutePath);
        await using var source = imageFile.OpenReadStream(10 * 1024 * 1024, cancellationToken);
        await source.CopyToAsync(destination, cancellationToken);

        return "/" + relativeFolder.Replace("\\", "/") + "/" + fileName;
    }

    private static void AddQuery(List<string> query, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add($"{name}={Uri.EscapeDataString(value)}");
        }
    }

    private sealed class AiSummaryResponse
    {
        public string? SummaryText { get; set; }
    }
}
