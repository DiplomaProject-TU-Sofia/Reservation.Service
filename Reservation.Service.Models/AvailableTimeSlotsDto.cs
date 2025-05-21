namespace Reservation.Service.Models
{
	public class AvailableTimeSlotsDto
	{
		public DateTime Date { get; set; }
		public List<TimeSpan> AvailableStartTimes { get; set; } = new();
	}
}
