using Reservation.Service.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reservation.Service.Data.Entities
{
	public class Saloon
	{
		public Guid Id { get; set; }

		public string Name { get; set; } = null!;

		public string Location { get; set; } = null!;
		
		[NotMapped]
		public Dictionary<DayOfWeek, WorkingHourRange> WorkHours { get; set; }

		//Navigation properties
		public ICollection<SaloonWorker> SaloonWorkers { get; set; } = new List<SaloonWorker>();
		public ICollection<WorkerService> WorkerServices { get; set; } = new List<WorkerService>();
	}
}
