namespace Reservation.Service.Models.Payment
{
	public class PaymentRequest
	{
		public decimal Amount { get; set; }

		public Guid ReservationId { get; set; }
	}

}
