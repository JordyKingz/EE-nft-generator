using EE.API.Workflow.Providers;
using EE.API.Workflow.Providers.Interfaces;
using EE.BL.Interfaces.Repositories;
using EE.BL.Services;
using EE.DAL.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace EE.API
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{

			services.AddControllers();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "EE.API", Version = "v1" });
			});

			//services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DatabaseConnection")));
			services.AddMemoryCache();			

			services.AddScoped<INftEntityRepository, JsonNftEntityRepository>();
			services.AddScoped<IStatsRepository, StatsRepository>();

			services.AddScoped<INftEntityService, NftEntityService>();

			services.AddScoped<IApiNftEntityProvider, ApiNftEntityProvider>();

			services.AddCors(o => o.AddPolicy("CorsCustomPolicy", builder =>
			{
				builder.AllowAnyOrigin()
								.AllowAnyMethod()
								.AllowAnyHeader();
			}));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EE.API v1"));
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseCors("CorsCustomPolicy");

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
