using LibraryManagement.BLL.DTO.Common;
using LibraryManagement.BLL.DTO.Reviews;
using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.Models;

namespace LibraryManagement.BLL.Services
{
    public class ReviewService
    {
        private readonly ReviewRepository reviewRepository;

        public ReviewService(ReviewRepository _reviewRepository)
        {
            reviewRepository = _reviewRepository;
        }

        public async Task<Book?> GetBookAsync(int bookId)
        {
            return await reviewRepository.GetBookAsync(bookId);
        }

        public async Task<List<BookReview>> GetBookReviewsAsync(int bookId)
        {
            return await reviewRepository.GetBookReviewsAsync(bookId);
        }

        public async Task<BookReview?> GetUserReviewAsync(int userId, int bookId)
        {
            return await reviewRepository.GetUserReviewAsync(userId, bookId);
        }

        public async Task<ActionResponse> SubmitAsync(ReviewSubmitRequest request)
        {
            if (request.Rating < 1 || request.Rating > 5)
            {
                return Fail("Rating must be from 1 to 5 stars.");
            }

            var account = await reviewRepository.GetMemberAccountAsync(request.UserId);
            if (account == null || account.Member == null)
            {
                return Fail("Only member accounts can review books.");
            }

            var book = await reviewRepository.GetBookAsync(request.BookId);
            if (book == null)
            {
                return Fail("Book not found.");
            }

            if (!await reviewRepository.HasBorrowedBookAsync(request.UserId, request.BookId))
            {
                return Fail("You can only review books that you have borrowed.");
            }

            var now = DateTime.UtcNow;
            var comment = string.IsNullOrWhiteSpace(request.Comment)
                ? null
                : request.Comment.Trim();
            var review = await reviewRepository.GetUserReviewAsync(request.UserId, request.BookId);

            if (review == null)
            {
                review = new BookReview
                {
                    UserId = request.UserId,
                    BookId = request.BookId,
                    Rating = request.Rating,
                    Comment = comment,
                    CreatedAt = now
                };
                reviewRepository.Add(review);
            }
            else
            {
                review.Rating = request.Rating;
                review.Comment = comment;
                review.UpdatedAt = now;
            }

            await reviewRepository.SaveChangesAsync();

            return new ActionResponse
            {
                IsSuccess = true,
                Message = "Review submitted successfully.",
                Id = review.BookReviewId
            };
        }

        private static ActionResponse Fail(string message)
        {
            return new ActionResponse
            {
                IsSuccess = false,
                Message = message
            };
        }
    }
}
