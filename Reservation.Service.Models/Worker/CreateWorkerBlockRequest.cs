namespace Reservation.Service.Models.Worker
{
    public class CreateWorkerBlockRequest
    {
        public string? BlockDescription { get; set; }

        public string? BlockTitle { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}
