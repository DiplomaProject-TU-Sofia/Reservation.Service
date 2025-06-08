using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.Service.Data.Repositories;
using Reservation.Service.Models.User;
using System.Text.Json;

namespace Reservation.Service.Controllers
{
	[Route("api/user-reservations")]
	[ApiController]
	[Authorize(Roles = "User")]
	public class UserReservationsController(ReservationRepository reservationRepository, ServiceBusClient serviceBusClient) : BaseController
	{
		private readonly string _emailQueueName = "confirmation-queue";

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
				var reservationId = await reservationRepository.CreateUserReservationAsync(request, userId);

				var reservation = await reservationRepository.GetReservationAsync(reservationId);

				var sender = serviceBusClient.CreateSender(_emailQueueName);

				var messageBody = JsonSerializer.Serialize(new
				{
					UserId = userId,
					Saloon = reservation.Saloon!.Name,
					Service = reservation.Service!.Name,
					Worker = reservation.Worker!.FirstName + " " + reservation.Worker!.LastName,
					reservation.StartTime,
					reservation.EndTime
				});

				var message = new ServiceBusMessage(messageBody);
				await sender.SendMessageAsync(message);
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
