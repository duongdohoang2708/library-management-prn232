using System.Net.Http.Json;
using System.Security.Claims;
using LibraryManagement.Blazor.Models;
using LibraryManagementDAL.Models;

namespace LibraryManagement.Blazor.Services;

public sealed class ReviewClientService : ApiClientBase
{
    public ReviewClientService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, configuration, httpContextAccessor)
    {
    }

    public async Task<ReviewPageViewModel?> GetReviewPageAsync(int bookId, CancellationToken cancellationToken = default)
    {
        var book = await GetAsync<Book>($"api/books/{bookId}", cancellationToken);
        if (book is null)
        {
            return null;
        }

        var reviews = await GetAsync<List<BookReview>>($"api/reviews/books/{bookId}", cancellationToken)
            ?? new List<BookReview>();

        BookReview? userReview = null;
        var userIdText = HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdText, out var userId))
        {
            using var client = CreateClient();
            var response = await client.GetAsync(BuildApiUrl($"api/reviews/books/{bookId}/users/{userId}"), cancellationToken);
            if (response.IsSuccessStatusCode && response.Content.Headers.ContentLength != 0)
            {
                userReview = await response.Content.ReadFromJsonAsync<BookReview?>(cancellationToken);
            }
        }

        return new ReviewPageViewModel
        {
            Book = book,
            Reviews = reviews,
            UserReview = userReview
        };
    }

    public async Task<OperationResult> SubmitReviewAsync(int bookId, int rating, string? comment, CancellationToken cancellationToken = default)
    {
        var userIdText = HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdText, out var userId))
        {
            return OperationResult.Failure("Please log in to submit a review.");
        }

        using var client = CreateClient();
        var response = await client.PostAsJsonAsync(
            BuildApiUrl("api/reviews"),
            new
            {
                userId,
                bookId,
                rating,
                comment
            },
            cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ActionResponse>(cancellationToken);
        return response.IsSuccessStatusCode
            ? OperationResult.Success(result?.Message ?? "Review submitted.")
            : OperationResult.Failure(result?.Message ?? "Submit review failed.");
    }

    private sealed class ActionResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? Id { get; set; }
    }
}
