using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Reservation.Service.Data;
using Reservation.Service.Data.Repositories;
using Azure.Messaging.ServiceBus;

var host = new HostBuilder()
	.ConfigureFunctionsWorkerDefaults()
	.ConfigureServices(services =>
	{
		services.AddDbContext<ReservationDbContext>(options =>
			options.UseSqlServer(Environment.GetEnvironmentVariable("SqlConnectionString")));

		services.AddScoped<ReservationRepository>();

		// Регистрация на ServiceBusClient (примерно)
		services.AddSingleton(serviceProvider =>
		{
			string connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
			return new ServiceBusClient(connectionString);
		});
	})
	.Build();

host.Run();
