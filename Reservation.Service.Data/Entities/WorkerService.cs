namespace Reservation.Service.Data.Entities
{
	public class WorkerService
	{
		public Guid UserId { get; set; }
		public User Worker { get; set; }

		public Guid ServiceId { get; set; }
		public Service Service { get; set; }
	}
}
