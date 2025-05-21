using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Reservation.Service.Data.Entities;
using Reservation.Service.Models;
using System.Text.Json;

namespace Reservation.Service.Data;

public class ReservationDbContext : DbContext
{
	public ReservationDbContext(DbContextOptions<ReservationDbContext> options)
		: base(options) { }
	

	public DbSet<Entities.Reservation> Reservations { get; set; }
	public DbSet<Saloon> Saloons { get; set; }
	public DbSet<Entities.Service> Services { get; set; }
	public DbSet<User> Users { get; set; }
	public DbSet<Role> Roles { get; set; }
	public DbSet<UserRole> UserRoles { get; set; }
	public DbSet<SaloonWorker> SaloonWorkers { get; set; }
	public DbSet<WorkerService> WorkerServices { get; set; }


	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Entities.Reservation>(builder =>
		{
			builder.HasKey(r => r.Id);

			builder.Property(r => r.StartTime)
				   .IsRequired();

			builder.Property(r => r.UserId)
				   .IsRequired(false);

			builder.Property(r => r.WorkerId)
				   .IsRequired();

			builder.Property(r => r.SaloonId)
				   .IsRequired(false);

			builder.Property(r => r.ServiceId)
				   .IsRequired(false);

			builder.HasIndex(r => new { r.WorkerId, r.StartTime });

			// User who made the reservation
			builder.HasOne<User>(r => r.User)
				   .WithMany(u => u.ReservationsMade)
				   .HasForeignKey(r => r.UserId)
				   .OnDelete(DeleteBehavior.Restrict)
				   .IsRequired(false);

			// Worker who will perform the service
			builder.HasOne<User>(r => r.Worker)
				   .WithMany(u => u.ReservationsAsWorker)
				   .HasForeignKey(r => r.WorkerId)
				   .OnDelete(DeleteBehavior.Restrict);

			builder.HasOne<Entities.Service>(r => r.Service)
				   .WithMany()
				   .HasForeignKey(r => r.ServiceId)
				   .OnDelete(DeleteBehavior.Restrict)
				   .IsRequired(false);

			builder.HasOne<Saloon>(r => r.Saloon)
				   .WithMany(s => s.Reservations)
				   .HasForeignKey(r => r.SaloonId)
				   .OnDelete(DeleteBehavior.Restrict)
					.IsRequired(false);
		});


		modelBuilder.Entity<User>(entity =>
		{
			entity.ToTable("AspNetUsers");
			entity.HasKey(u => u.Id);

			entity.HasMany(u => u.UserRoles)
				  .WithOne(ur => ur.User)
				  .HasForeignKey(ur => ur.UserId);

			entity.Metadata.SetIsTableExcludedFromMigrations(true);
		});

		modelBuilder.Entity<Role>(entity =>
		{
			entity.ToTable("AspNetRoles");
			entity.HasKey(r => r.Id);

			entity.Metadata.SetIsTableExcludedFromMigrations(true);
		});

		modelBuilder.Entity<UserRole>(entity =>
		{
			entity.ToTable("AspNetUserRoles");
			entity.HasKey(ur => new { ur.UserId, ur.RoleId });

			entity.HasOne(ur => ur.User)
				  .WithMany(u => u.UserRoles)
				  .HasForeignKey(ur => ur.UserId);

			entity.HasOne(ur => ur.Role)
				  .WithMany()
				  .HasForeignKey(ur => ur.RoleId);

			entity.Metadata.SetIsTableExcludedFromMigrations(true);
		});


		// Many-to-many relationship between Worker and Saloon
		modelBuilder.Entity<SaloonWorker>()
			.HasKey(sw => new { sw.UserId, sw.SaloonId });

		modelBuilder.Entity<SaloonWorker>()
			.HasOne(sw => sw.Worker)
			.WithMany(w => w.SaloonWorkers)
			.HasForeignKey(sw => sw.UserId);

		modelBuilder.Entity<SaloonWorker>()
			.HasOne(sw => sw.Saloon)
			.WithMany(s => s.SaloonWorkers)
			.HasForeignKey(sw => sw.SaloonId);

		// Many-to-many relationship between Worker and Service
		modelBuilder.Entity<WorkerService>()
			.HasKey(ws => new { ws.UserId, ws.ServiceId });

		modelBuilder.Entity<WorkerService>()
			.HasOne(ws => ws.Worker)
			.WithMany(w => w.WorkerServices)
			.HasForeignKey(ws => ws.UserId);

		modelBuilder.Entity<WorkerService>()
			.HasOne(ws => ws.Service)
			.WithMany(s => s.WorkerServices)
			.HasForeignKey(ws => ws.ServiceId);

		var daysOfWeekConverter = new ValueConverter<IEnumerable<DayOfWeek>, string>(
			v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
			v => JsonSerializer.Deserialize<IEnumerable<DayOfWeek>>(v, (JsonSerializerOptions?)null) ?? new List<DayOfWeek>());

		modelBuilder.Entity<SaloonWorker>()
			.Property(sw => sw.WorkingDays)
			.HasConversion(daysOfWeekConverter);

		var workHoursConverter = new ValueConverter<Dictionary<DayOfWeek, WorkingHourRange>, string>(
			v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
			v => JsonSerializer.Deserialize<Dictionary<DayOfWeek, WorkingHourRange>>(v ?? "{}", (JsonSerializerOptions?)null) ?? new());

		var workHoursComparer = new ValueComparer<Dictionary<DayOfWeek, WorkingHourRange>>(
			(c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(c2, (JsonSerializerOptions?)null),
			c => JsonSerializer.Serialize(c, (JsonSerializerOptions?)null).GetHashCode(),
			c => JsonSerializer.Deserialize<Dictionary<DayOfWeek, WorkingHourRange>>(JsonSerializer.Serialize(c, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null) ?? new()
		);

		modelBuilder.Entity<Saloon>()
			.Property(s => s.WorkHours)
			.HasConversion(workHoursConverter)
			.Metadata.SetValueComparer(workHoursComparer);

	}
}
