namespace Reservation.Service.Models.User
{
    public class CreateUserReservationRequest
    {
        public Guid ServiceId { get; set; }
        public Guid SaloonId { get; set; }
        public Guid WorkerId { get; set; }
        public DateTime StartTime { get; set; }
    }
}
