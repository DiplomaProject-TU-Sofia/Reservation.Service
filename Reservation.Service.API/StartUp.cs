using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Reservation.Service.Data;
using Reservation.Service.Data.Repositories;
using System.Text;

public class Startup
{
	public IConfiguration Configuration { get; }
	public Startup(IConfiguration configuration) => Configuration = configuration;

	public void ConfigureServices(IServiceCollection services)
	{
		services.AddDbContext<ReservationDbContext>(options =>
			options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

		services.AddControllers();
		
		// Enable JWT authentication
		// Add Authentication using JWT Bearer
		services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidIssuer = Configuration["JwtSettings:Issuer"],
				ValidateAudience = true,
				ValidAudience = Configuration["JwtSettings:Audience"],
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(
					Encoding.UTF8.GetBytes(Configuration["JwtSettings:SecretKey"])
				),
				RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
			};
		});

		services.AddAuthorization();

		services.AddTransient<ReservationRepository>();

		services.AddCors(options =>
		{
			options.AddPolicy("AllowAll", builder =>
			{
				builder
					.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader();
			});
		});
	}

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}

		app.UseRouting();

		app.UseCors("AllowAll");
		// Enable Authentication & Authorization Middleware
		app.UseAuthentication();
		app.UseAuthorization();

		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
		});
	}
}
