using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Reservation.Service.Controllers
{
	[ApiController]
	public class BaseController : ControllerBase
	{
		public Guid GetContextUserId()
		{
			var userIdClaim = User.Claims.FirstOrDefault(c =>
				c.Type == ClaimTypes.NameIdentifier ||
				c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

			if (userIdClaim == null)
			{
				// Handle missing claim, e.g. return Unauthorized or throw
				throw new Exception("User Id claim not found");
			}

			var workerId = Guid.Parse(userIdClaim.Value);
			return workerId;
		}
	}
}
