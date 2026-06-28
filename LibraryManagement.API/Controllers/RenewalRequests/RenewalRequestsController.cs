using LibraryManagement.BLL.Services;
using LibraryManagementDAL.DTO.RenewalRequest;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.RenewalRequests
{
    [ApiController]
    [Route("api/renewal-requests")]
    public class RenewalRequestsController : ControllerBase
    {
        private readonly RenewalRequestService renewalRequestService;

        public RenewalRequestsController(RenewalRequestService _renewalRequestService)
        {
            renewalRequestService = _renewalRequestService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPending()
        {
            return Ok(await renewalRequestService.GetPendingForStaffAsync());
        }

        [HttpGet("users/{userId:int}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            return Ok(await renewalRequestService.GetByUserAsync(userId));
        }

        [HttpPost]
        public async Task<IActionResult> Create(RenewalRequestCreateRequest request)
        {
            var result = await renewalRequestService.CreateRequestAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id, RenewalRequestApproveRequest request)
        {
            var result = await renewalRequestService.ApproveAsync(id, request.ReviewerUserId);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id, RenewalRequestRejectRequest request)
        {
            var result = await renewalRequestService.RejectAsync(id, request.ReviewerUserId, request.Reason);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, [FromQuery] int userId)
        {
            var result = await renewalRequestService.CancelAsync(id, userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
