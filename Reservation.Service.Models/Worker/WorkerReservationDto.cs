namespace Reservation.Service.Models.Worker
{
	public class WorkerReservationDto
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public bool IsBlock { get; set; }

		// If IsBlock == true
		public string? BlockTitle { get; set; }
		public string? BlockDescription { get; set; }

		// If IsBlock == false
		public string? ClientFirstName { get; set; }
		public string? ClientLastName { get; set; }
		public string? ServiceName { get; set; }
		public string? SaloonName { get; set; }
		public string? SaloonLocation { get; set; }
	}
}
