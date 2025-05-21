namespace Reservation.Service.Data.Entities
{
	public class SaloonWorker
	{
		public Guid UserId { get; set; }
		public Guid SaloonId { get; set; }
		public IEnumerable<DayOfWeek> WorkingDays { get; set; } = new List<DayOfWeek>();


		// Navigation properties
		public Saloon Saloon { get; set; }
		public User Worker { get; set; }
	}
}
