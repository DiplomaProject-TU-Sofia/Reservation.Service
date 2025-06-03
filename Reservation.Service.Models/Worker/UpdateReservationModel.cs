namespace Reservation.Service.Models.Worker
{
	public class UpdateReservationModel
	{
		public Guid Id { get; set; }

		public DateTime StartTime { get; set; }

		public DateTime EndTime { get; set; }

	}
}