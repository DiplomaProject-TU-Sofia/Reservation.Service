namespace Reservation.Service.Models.Worker
{
	public class WorkerReservationDto
	{
		public Guid Id { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string? BlockTitle { get; set; }
		public string? BlockDescription { get; set; }
	}
}
