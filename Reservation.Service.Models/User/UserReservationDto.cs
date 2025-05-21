namespace Reservation.Service.Models.User
{
    public class UserReservationDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string WorkerFirstName { get; set; }
        public string WorkerLastName { get; set; }

        public string ServiceName { get; set; }

        public string SaloonName { get; set; }
        public string SaloonLocation { get; set; }
    }

}
