using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.Service.Data.Repositories;
using Reservation.Service.Models.Payment;
using Reservation.Service.Models.User;
using Stripe;
using System.Text.Json;

namespace Reservation.Service.Controllers
{
	[Route("api/user-reservations")]
	[ApiController]
	[Authorize(Roles = "User")]
	public class UserReservationsController(ReservationRepository reservationRepository, ServiceBusClient serviceBusClient, IConfiguration configuration) : BaseController
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

			var reservationId = Guid.Empty;
			try
			{
				reservationId = await reservationRepository.CreateUserReservationAsync(request, userId);
				
				var reservation = await reservationRepository.GetReservationAsync(reservationId);

				var sender = serviceBusClient.CreateSender(configuration["ServiceBusConfirmationQueue"]);

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

			return Ok(reservationId);
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

		[HttpPost("create-payment-intent")]
		public ActionResult CreatePaymentIntent([FromBody] PaymentRequest request)
		{
			var options = new PaymentIntentCreateOptions
			{
				Amount = (long)(request.Amount * 100),
				Currency = "bgn",
				AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
				{
					Enabled = true,
				},
				Metadata = new Dictionary<string, string>
				{
					{ "reservationId", request.ReservationId.ToString() }
				}
			};

			var service = new PaymentIntentService();
			var intent = service.Create(options);

			return Ok(new { clientSecret = intent.ClientSecret });
		}

		[HttpPost("webhook")]
		public async Task<IActionResult> StripeWebhook()
		{
			var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

			try
			{
				var stripeEvent = EventUtility.ConstructEvent(
					json,
					Request.Headers["Stripe-Signature"],
					configuration["Stripe:WebhookSecret"]
				);

				if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
				{
					var paymentIntent = (PaymentIntent)stripeEvent.Data.Object;

					if (paymentIntent.Metadata.TryGetValue("reservationId", out var reservationId))
					{
						await reservationRepository.UpdateReservationPaid(Guid.Parse(reservationId));
					}
				}

				return Ok();
			}
			catch (StripeException ex)
			{
				Console.WriteLine($"Stripe webhook error: {ex.Message}");
				return BadRequest();
			}
		}
	}
}
