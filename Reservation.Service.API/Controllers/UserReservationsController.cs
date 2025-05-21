using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.Service.Data.Repositories;
using Reservation.Service.Models.User;
using System.Security.Claims;

namespace Reservation.Service.Controllers
{
	[Route("api/user-reservations")]
	[ApiController]
	[Authorize(Roles = "User")]
	public class UserReservationsController(ReservationRepository reservationRepository) : ControllerBase
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

			var userIdClaim = User.Claims.FirstOrDefault(c =>
				c.Type == ClaimTypes.NameIdentifier ||
				c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

			if (userIdClaim == null)
			{
				// Handle missing claim, e.g. return Unauthorized or throw
				throw new Exception("User Id claim not found");
			}

			var userId = Guid.Parse(userIdClaim.Value);

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
			var userIdClaim = User.Claims.FirstOrDefault(c =>
				c.Type == ClaimTypes.NameIdentifier ||
				c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

			if (userIdClaim == null)
			{
				// Handle missing claim, e.g. return Unauthorized or throw
				throw new Exception("User Id claim not found");
			}

			var userId = Guid.Parse(userIdClaim.Value);

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
