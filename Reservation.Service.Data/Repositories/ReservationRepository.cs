using Microsoft.EntityFrameworkCore;
using Reservation.Service.Models;
using Reservation.Service.Models.User;
using Reservation.Service.Models.Worker;

namespace Reservation.Service.Data.Repositories
{
	public class ReservationRepository
	{
		private readonly ReservationDbContext _context;

		public ReservationRepository(ReservationDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<Entities.Reservation>> GetReservationsForWorkerAsync(Guid workerId, DateTime from, DateTime to)
		{
			return await _context.Reservations
				.Where(r => r.WorkerId == workerId && r.StartTime >= from && r.StartTime < to)
				.ToListAsync();
		}

		public async Task AddReservationAsync(Entities.Reservation reservation)
		{
			await _context.Reservations.AddAsync(reservation);
			await _context.SaveChangesAsync();
		}

		public async Task<bool> IsWorkerAvailableAsync(Guid workerId, DateTime startTime, TimeSpan duration)
		{
			var endTime = startTime + duration;

			return !await _context.Reservations.AnyAsync(r =>
				r.WorkerId == workerId &&
				r.StartTime < endTime &&
				(r.StartTime + duration) > startTime);
		}

		public async Task CreateUserReservationAsync(CreateUserReservationRequest request, Guid userId)
		{
			var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == request.ServiceId);
			if (service == null)
				throw new Exception($"Couldn't find any service with Id == {request.ServiceId}");

			var startTime = request.StartTime;
			var endTime = startTime.Add(service.Duration);
			var dayOfWeek = startTime.DayOfWeek;

			// Load saloon with work hours (deserialize manually if needed)
			var saloon = await _context.Saloons
				.Include(s => s.SaloonWorkers)
				.FirstOrDefaultAsync(s => s.Id == request.SaloonId);

			if (saloon == null)
				throw new Exception($"Saloon with id {request.SaloonId} not found.");

			// Check if saloon is open during the requested time
			if (!saloon.WorkHours.TryGetValue(dayOfWeek, out var workingHours))
				throw new Exception("The saloon is closed on the selected day.");

			if (startTime.TimeOfDay < workingHours.Open || endTime.TimeOfDay > workingHours.Close)
				throw new Exception("The reservation time is outside of the saloon's working hours.");

			// Check if worker is working that day at this saloon
			var saloonWorker = saloon.SaloonWorkers.FirstOrDefault(sw => sw.UserId == request.WorkerId && sw.SaloonId == request.SaloonId);
			if (saloonWorker == null)
				throw new Exception("The selected worker is not part of the saloon.");

			if (!saloonWorker.WorkingDays.Contains(dayOfWeek))
				throw new Exception("The worker is not working on the selected day.");

			// Check if the worker has any overlapping reservations
			var conflictExists = await _context.Reservations.AnyAsync(r =>
				r.WorkerId == request.WorkerId &&
				((startTime >= r.StartTime && startTime < r.EndTime) ||   // starts during another reservation
				 (endTime > r.StartTime && endTime <= r.EndTime) ||       // ends during another reservation
				 (startTime <= r.StartTime && endTime >= r.EndTime))      // fully overlaps another reservation
			);

			if (conflictExists)
				throw new Exception("The worker already has a reservation during the requested time.");

			// All validations passed, create reservation
			var reservation = new Entities.Reservation
			{
				Id = Guid.NewGuid(),
				SaloonId = request.SaloonId,
				ServiceId = request.ServiceId,
				WorkerId = request.WorkerId,
				UserId = userId,
				StartTime = startTime,
				EndTime = endTime
			};

			_context.Reservations.Add(reservation);
			await _context.SaveChangesAsync();
		}


		public async Task CreateWorkerBlockAsync(CreateWorkerBlockRequest request, Guid workerId)
		{
			var startTime = request.StartTime;
			var endTime = request.EndTime;

			// Check if the worker has any overlapping reservations
			var conflictExists = await _context.Reservations.AnyAsync(r =>
				r.WorkerId == workerId &&
				((startTime >= r.StartTime && startTime < r.EndTime) ||   // starts during another reservation
				 (endTime > r.StartTime && endTime <= r.EndTime) ||       // ends during another reservation
				 (startTime <= r.StartTime && endTime >= r.EndTime))      // fully overlaps another reservation
			);

			if (conflictExists)
				throw new Exception("The worker already has a reservation during the requested time.");

			var reservation = new Entities.Reservation
			{
				Id = Guid.NewGuid(),
				StartTime = startTime,
				EndTime = endTime,
				IsBlock = true,
				BlockDescription = request.BlockDescription,
				BlockTitle = request.BlockTitle,
				WorkerId = workerId
			};

			await _context.Reservations.AddAsync(reservation);
			await _context.SaveChangesAsync();
		}

		public async Task<List<UserReservationsByDateDto>> GetUserReservationsAsync(Guid userId, DateTime? from, DateTime? to)
		{
			var query = _context.Reservations
				.Include(r => r.Worker)
				.Include(r => r.Service)
				.Include(r => r.Saloon)
				.Where(r => r.UserId == userId);

			if (from.HasValue)
				query = query.Where(r => r.StartTime >= from.Value);

			if (to.HasValue)
				query = query.Where(r => r.StartTime <= to.Value);

			var reservations = await query
				.AsNoTracking()
				.ToListAsync();

			var grouped = reservations
				.GroupBy(r => DateOnly.FromDateTime(r.StartTime))
				.OrderBy(g => g.Key)
				.Select(g => new UserReservationsByDateDto
				{
					Date = g.Key,
					Reservations = g.Select(r => new UserReservationDto
					{
						StartTime = r.StartTime,
						EndTime = r.EndTime,
						WorkerFirstName = r.Worker.FirstName,
						WorkerLastName = r.Worker.LastName,
						ServiceName = r.Service?.Name ?? string.Empty,
						SaloonName = r.Saloon?.Name ?? string.Empty,
						SaloonLocation = r.Saloon?.Location ?? string.Empty
					}).ToList()
				}).ToList();

			return grouped;
		}

		public async Task<List<WorkerReservationsByDateDto>> GetWorkerReservationsAsync(Guid workerId, DateTime? from, DateTime? to)
		{
			var query = _context.Reservations
				.Include(r => r.User)
				.Include(r => r.Service)
				.Include(r => r.Saloon)
				.Where(r => r.WorkerId == workerId);

			if (from.HasValue)
				query = query.Where(r => r.StartTime >= from.Value);
			if (to.HasValue)
				query = query.Where(r => r.StartTime <= to.Value);

			var reservations = await query
				.AsNoTracking()
				.ToListAsync();

			var grouped = reservations
				.GroupBy(r => DateOnly.FromDateTime(r.StartTime))
				.OrderBy(g => g.Key)
				.Select(g => new WorkerReservationsByDateDto
				{
					Date = g.Key,
					Reservations = g.Select(r => new WorkerReservationDto
					{
						Id = r.Id,
						StartTime = r.StartTime,
						EndTime = r.EndTime,
						BlockTitle = r.IsBlock ? r.BlockTitle : r.Service!.Name,
						BlockDescription = r.IsBlock ? r.BlockDescription : r.Saloon!.Name
					}).ToList()
				}).ToList();

			return grouped;
		}

		public async Task<List<AvailableTimeSlotsDto>> GetAvailableTimeSlotsAsync(
			Guid serviceId,
			Guid saloonId,
			Guid workerId,
			DateTime from,
			DateTime to)
		{
			var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == serviceId)
				?? throw new Exception("Service not found.");

			var saloon = await _context.Saloons.FirstOrDefaultAsync(s => s.Id == saloonId)
				?? throw new Exception("Saloon not found.");

			if (saloon.WorkHours == null)
				throw new Exception("Saloon working hours not configured.");

			var saloonWorker = await _context.SaloonWorkers.FirstOrDefaultAsync(sw =>
				sw.SaloonId == saloonId && sw.UserId == workerId);

			if (saloonWorker == null)
				throw new Exception("Worker does not work at this saloon.");

			var canDoService = await _context.WorkerServices.AnyAsync(ws =>
				ws.UserId == workerId && ws.ServiceId == serviceId);

			if (!canDoService)
				throw new Exception("Worker is not assigned to this service.");

			var allReservations = await _context.Reservations
				.Where(r =>
					r.WorkerId == workerId &&
					r.StartTime >= from.Date &&
					r.EndTime <= to.Date.AddDays(1))
				.ToListAsync();

			var result = new List<AvailableTimeSlotsDto>();
			var duration = service.Duration;
			var step = TimeSpan.FromMinutes(15); // adjustable step

			for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
			{
				var slots = new AvailableTimeSlotsDto { Date = date };

				if (!saloon.WorkHours.TryGetValue(date.DayOfWeek, out var workingHours))
					continue;

				if (!saloonWorker.WorkingDays.Contains(date.DayOfWeek))
					continue;

				var reservations = allReservations
					.Where(r => r.StartTime.Date == date)
					.OrderBy(r => r.StartTime)
					.ToList();

				var start = workingHours.Open;
				var end = workingHours.Close;

				var current = start;
				while (current + duration <= end)
				{
					var slotStart = date + current;
					var slotEnd = slotStart + duration;

					var hasConflict = reservations.Any(r =>
						(slotStart >= r.StartTime && slotStart < r.EndTime) ||
						(slotEnd > r.StartTime && slotEnd <= r.EndTime) ||
						(slotStart <= r.StartTime && slotEnd >= r.EndTime));

					if (!hasConflict)
						slots.AvailableStartTimes.Add(current);

					current = current.Add(step);
				}

				if (slots.AvailableStartTimes.Count != 0)
					result.Add(slots);
			}

			return result;
		}


	}
}
