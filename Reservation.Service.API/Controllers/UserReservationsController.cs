using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.Service.Data.Repositories;
using Reservation.Service.Models.User;

namespace Reservation.Service.Controllers
{
	[Route("api/user-reservations")]
	[ApiController]
	[Authorize(Roles = "User")]
	public class UserReservationsController(ReservationRepository reservationRepository) : BaseController
	{
		[HttpPost()]
		public async Task<IActionResult> CreateReservationAsUser([FromBody] CreateUserReservationRequest request)
		{
			if (request == null ||
				request.WorkerId == default ||
				request.ServiceId == default ||
				request.SaloonId == default ||
				request.StartTime == default)
			{
				return BadRequest();
			}
		
			var userId = GetContextUserId();

			try
			{
				await reservationRepository.CreateUserReservationAsync(request, userId);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}

			return Ok();
		}

		[HttpGet()]
		public async Task<IActionResult> GetUserReservations([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
		{
			var userId = GetContextUserId();

			var reservations = await reservationRepository.GetUserReservationsAsync(userId, from, to);

			return Ok(reservations);
		}

		[HttpGet("available-timeslots")]
		public async Task<IActionResult> GetAvailableTimeSlots(
			Guid serviceId,
			Guid saloonId,
			Guid workerId,
			DateTime from,
			DateTime to)
		{
			var result = await reservationRepository.GetAvailableTimeSlotsAsync(serviceId, saloonId, workerId, from, to);
			
			return Ok(result);
		}


	}
}
