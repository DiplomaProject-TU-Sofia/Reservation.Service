namespace Reservation.Service.Models
{
	public class ReservationReminderEmailInfo
	{
		public Guid UserId { get; set; }
		public string Service { get; set; }
		public string Saloon { get; set; }
		public DateTime StartTime { get; set; }
	}

}
