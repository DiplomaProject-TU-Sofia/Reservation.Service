using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.Service.Data.Repositories;
using Reservation.Service.Models.Worker;

namespace Reservation.Service.Controllers
{
	[Route("api/worker-reservations")]
	[ApiController]
	public class WorkerReservationsController(ReservationRepository reservationRepository) : BaseController
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

			var workerId = GetContextUserId();
			
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
			var workerId = GetContextUserId();

			var reservations = await reservationRepository.GetWorkerReservationsAsync(workerId, from, to);

			return Ok(reservations);
		}
	}
}
