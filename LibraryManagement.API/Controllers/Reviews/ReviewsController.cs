using LibraryManagement.BLL.DTO.Reviews;
using LibraryManagement.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Reviews
{
    [ApiController]
    [Route("api/reviews")]
    public class ReviewsController : ControllerBase
    {
        private readonly ReviewService reviewService;

        public ReviewsController(ReviewService _reviewService)
        {
            reviewService = _reviewService;
        }

        [HttpGet("books/{bookId:int}")]
        public async Task<IActionResult> GetBookReviews(int bookId)
        {
            return Ok(await reviewService.GetBookReviewsAsync(bookId));
        }

        [HttpGet("books/{bookId:int}/users/{userId:int}")]
        public async Task<IActionResult> GetUserReview(int bookId, int userId)
        {
            return Ok(await reviewService.GetUserReviewAsync(userId, bookId));
        }

        [HttpPost]
        public async Task<IActionResult> Submit(ReviewSubmitRequest request)
        {
            var result = await reviewService.SubmitAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
