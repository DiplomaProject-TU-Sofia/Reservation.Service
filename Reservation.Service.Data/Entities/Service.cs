namespace Reservation.Service.Data.Entities
{
	public class Service
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;
		public string? Description { get; set; }
		public decimal Price { get; set; }
		public TimeSpan Duration { get; set; }

		// Navigation properties
		public ICollection<WorkerService> WorkerServices { get; set; } = new List<WorkerService>();
	}
}
