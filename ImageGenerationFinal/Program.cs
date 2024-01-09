using EE.BL.Services;
using ImageGenerationFinal.Workflow.Processors;
using ImageGenerationFinal.Workflow.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ImageGenerationFinal
{
	class Program
	{ 
    static async Task Main(string[] args)
		{
			using IHost host = CreateHostBuilder(args).Build();

			await ExemplifyScoping(host.Services, "Scope 1").ConfigureAwait(true);			
		}										

		static IHostBuilder CreateHostBuilder(string[] args) =>
						Host.CreateDefaultBuilder(args)
								.ConfigureServices((_, services) =>
								{
									services.AddMemoryCache();									
									services.AddScoped<INftEntityService, NftEntityService>();

									services.AddScoped<ImageGenerationProvider, ImageGenerationProvider>();
									services.AddScoped<IImageGenerationUploader, ImageGenerationUploader>();
								});

		static async Task ExemplifyScoping(IServiceProvider services, string scope)
		{
			using IServiceScope serviceScope = services.CreateScope();
			IServiceProvider provider = serviceScope.ServiceProvider;

			var imageGenerationUploader = provider.GetService<IImageGenerationUploader>();
			await imageGenerationUploader.Process().ConfigureAwait(false);			
			Console.ReadKey();
		}
	}	
}
