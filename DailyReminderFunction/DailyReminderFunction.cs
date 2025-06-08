using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Reservation.Service.Data.Repositories;
using Reservation.Service.Models;
using System.Text.Json;

namespace DailyReminderFunction
{
	public class DailyReminderFunction
	{
		private readonly ReservationRepository _reservationRepository;
		private readonly ServiceBusClient _serviceBusClient;
		private readonly ILogger _logger;

		public DailyReminderFunction(
			ReservationRepository reservationRepository,
			ServiceBusClient serviceBusClient,
			ILogger<DailyReminderFunction> logger)
		{
			_reservationRepository = reservationRepository;
			_serviceBusClient = serviceBusClient;
			_logger = logger;
		}

		[Function("DailyReminderFunction")]
		public async Task RunAsync([TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo)
		{
			_logger.LogInformation("Running daily reminder function at: {Time}", DateTime.UtcNow);

			var reservations = await _reservationRepository.GetReservationsForNextDayAsync();

			if (reservations.Count == 0)
			{
				_logger.LogInformation("No upcoming reservations for tomorrow.");
				return;
			}

			var sender = _serviceBusClient.CreateSender("reminder-queue");

			foreach (var reservation in reservations)
			{
				var reminder = new ReservationReminderEmailInfo
				{
					UserId = reservation.UserId!.Value,
					Service = reservation.Service!.Name,
					Saloon = reservation.Saloon!.Name,
					StartTime = reservation.StartTime
				};

				var message = new ServiceBusMessage(JsonSerializer.Serialize(reminder));
				await sender.SendMessageAsync(message);
			}

			_logger.LogInformation("Sent {Count} reminder emails to Service Bus", reservations.Count);
		}
	}
}
