using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.Service.Data.Repositories;
using Reservation.Service.Models.Worker;
using System.Security.Claims;

namespace Reservation.Service.Controllers
{
	[Route("api/worker-reservations")]
	[ApiController]
	[AllowAnonymous]
	public class WorkerReservationsController(ReservationRepository reservationRepository) : ControllerBase
	{
		[HttpPost]
		public async Task<IActionResult> CreateBlockAsWorker([FromBody] CreateWorkerBlockRequest request)
		{
			if (request == null ||
				request.StartTime == default ||
				request.EndTime == default)
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

			var workerId = Guid.Parse(userIdClaim.Value);
			
			try
			{
				await reservationRepository.CreateWorkerBlockAsync(request, workerId);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}

			return Ok();
		}

		[HttpGet]
		public async Task<IActionResult> GetWorkerReservations([FromQuery] DateTime? from, [FromQuery] DateTime? to)
		{
			var userIdClaim = User.Claims.FirstOrDefault(c =>
				c.Type == ClaimTypes.NameIdentifier ||
				c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

			if (userIdClaim == null)
			{
				// Handle missing claim, e.g. return Unauthorized or throw
				throw new Exception("User Id claim not found");
			}

			var workerId = Guid.Parse(userIdClaim.Value);

			var reservations = await reservationRepository.GetWorkerReservationsAsync(workerId, from, to);

			return Ok(reservations);
		}
	}
}
