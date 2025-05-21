namespace Reservation.Service.Data.Entities
{
	public class Reservation
	{
		public Guid Id { get; set; }

		public Guid? UserId { get; set; }
		public User? User { get; set; }

		public Guid WorkerId { get; set; }
		public User Worker { get; set; }

		public Guid? SaloonId { get; set; }
		public Saloon? Saloon { get; set; }

		public Guid? ServiceId { get; set; }
		public Service? Service { get; set; }

		public DateTime StartTime { get; set; }

		public DateTime EndTime { get; set; }

		public string? BlockDescription { get; set; }

		public string? BlockTitle { get; set; }

		public bool IsBlock { get; set; } = false;
	}
}
