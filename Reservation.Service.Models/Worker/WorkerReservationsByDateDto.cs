namespace Reservation.Service.Models.Worker
{
	public class WorkerReservationsByDateDto
	{
		public DateOnly Date { get; set; }
		public List<WorkerReservationDto> Reservations { get; set; }
	}
}
