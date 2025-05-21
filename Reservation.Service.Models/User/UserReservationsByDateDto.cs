namespace Reservation.Service.Models.User
{
    public class UserReservationsByDateDto
    {
        public DateOnly Date { get; set; }
        public List<UserReservationDto> Reservations { get; set; }
    }

}
