using LibraryManagement.BLL.Services;
using LibraryManagementDAL.DTO.Reservation;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Reservations
{
    [ApiController]
    [Route("api/reservations")]
    public class ReservationsController : ControllerBase
    {
        private readonly ReservationService reservationService;

        public ReservationsController(ReservationService _reservationService)
        {
            reservationService = _reservationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetReservations()
        {
            return Ok(await reservationService.GetReservationsAsync());
        }

        [HttpGet("users/{userId:int}")]
        public async Task<IActionResult> GetUserReservations(int userId)
        {
            return Ok(await reservationService.GetUserReservationsAsync(userId));
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation(ReservationCreateRequest request)
        {
            var result = await reservationService.CreateReservationAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await reservationService.ApproveAsync(id);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await reservationService.CancelAsync(id);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
