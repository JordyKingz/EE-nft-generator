using ImageGenerationFinal.Workflow.Providers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ImageGenerationLocal
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var provider = new ImageGenerationProvider();
			var dir = $"{Directory.GetCurrentDirectory()}/resources";
#if DEBUG
			dir = @"C:/imageresources";
#endif
			var result = await provider.Provide(dir).ConfigureAwait(false);

			foreach (var item in result)
			{
				Console.WriteLine($"Writing image {item.GeneratedImageId} to local dir: {dir}/generated/");
				Directory.CreateDirectory($"{dir}/generated/");
				item.generatedImageBitmap.Save($"{dir}/generated/{item.GeneratedImageId}.png");
			}
			Console.WriteLine("Done writing images!!!");
			Console.WriteLine("press any key to exit...");
			Console.ReadKey();
		}
	}
}
