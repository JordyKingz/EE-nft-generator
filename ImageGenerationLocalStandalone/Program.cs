using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGenerationLocalStandalone
{
	class Program
	{
		static async Task Main(string[] args)
		{
			//			var stopwatch = new Stopwatch();
			//			stopwatch.Start();
			//			Console.WriteLine("!!!! ---- Money printer9000 starting ;) ---- !!!!");
			//			var provider = new ImageGenerationProvider();
			//			var dir = $"{Directory.GetCurrentDirectory()}/resources";
			//#if DEBUG
			//			dir = @"C:/imageresources";
			//#endif
			//			var result = await provider.Provide(dir).ConfigureAwait(false);

			//			foreach (var item in result)
			//			{
			//				Console.WriteLine($"Writing image {item.GeneratedImageId} to local dir: {dir}/generated/");
			//				Directory.CreateDirectory($"{dir}/generated/");
			//				item.generatedImageBitmap.Save($"{dir}/generated/{item.GeneratedImageId}.png");
			//			}
			//			stopwatch.Stop();
			//			Console.WriteLine($"took {stopwatch.Elapsed}");
			//			Console.WriteLine("Done writing images!!!");
			//			Console.WriteLine("press any key to exit...");
			//			Console.ReadKey();

			var stopwatch = new Stopwatch();
			stopwatch.Start();
			var provider = new ImageGenerationProvider();
			var dir = $"{Directory.GetCurrentDirectory()}/resources";
#if DEBUG
			dir = @"C:/imageresources";
#endif

			var backgrounds = await provider.GetImages(TraitType.Background, dir, "backgrounds").ConfigureAwait(false);
			var baseForms = await provider.GetImages(TraitType.Baseform, dir, "baseforms").ConfigureAwait(false);
			var hair = await provider.GetImages(TraitType.Hair, dir, "hair").ConfigureAwait(false);
			var faces = await provider.GetImages(TraitType.Face, dir, "faces").ConfigureAwait(false);
			var outfits = await provider.GetImages(TraitType.Outfit, dir, "outfits").ConfigureAwait(false);
			var result = await provider.ProvideBase(dir, backgrounds, baseForms, hair, faces, outfits).ConfigureAwait(false);			

			foreach (var item in result)
			{
				Console.WriteLine($"Writing image {item.GeneratedImageId} to local dir: {dir}/generated/");
				Directory.CreateDirectory($"{dir}/generated/");
				var background = new Bitmap(backgrounds.First(x => x.Identifier == item.BackgroundIdentifier).Image);
				var backgroundRect = new Rectangle(Point.Empty, background.Size);

				using (var gr = Graphics.FromImage(background))
				{
					gr.DrawImageUnscaledAndClipped(baseForms.First(x => x.Identifier == item.BaseformIdentifier).Image, backgroundRect);
					gr.DrawImageUnscaledAndClipped(faces.First(x => x.Identifier == item.FaceIdentifier).Image, backgroundRect);
					gr.DrawImageUnscaledAndClipped(outfits.First(x => x.Identifier == item.OutfitIdentifier).Image, backgroundRect);
					gr.DrawImageUnscaledAndClipped(hair.First(x => x.Identifier == item.HairIdentifier).Image, backgroundRect);
				}
				background.Save($"{dir}/generated/{item.GeneratedImageId}.png");
			}
			stopwatch.Stop();
			Console.WriteLine($"took {stopwatch.Elapsed}");
			Console.WriteLine("Done writing images!!!");
			Console.WriteLine("press any key to exit...");
			Console.ReadKey();
		}
	}

	public class ImageGenerationProvider
	{		
		public async Task<List<GeneratedImage>> ProvideBase(string dir, List<TraitImage> backgrounds, List<TraitImage> baseForms, List<TraitImage> hair, List<TraitImage> faces, List<TraitImage> outfits)
		{
			Console.WriteLine($"Generating images from dir: {dir}");			

			var maxnr = outfits.Count * hair.Count * baseForms.Count * faces.Count;
			var rand = new Random();
			var generated = new List<GeneratedImage>();
			int counter = 1;
			Console.WriteLine($"Maximum number of unique combinations: {maxnr}");

			while (counter < maxnr + 1)
			{
				int backgroundId = rand.Next(0, backgrounds.Count);
				int baseformId = rand.Next(0, baseForms.Count);
				int faceId = rand.Next(0, faces.Count);
				int hairId = rand.Next(0, hair.Count);
				int outfitId = rand.Next(0, outfits.Count);

				var generatedBackground = backgrounds[backgroundId];
				var generatedBaseform = baseForms[baseformId];
				var generatedFace = faces[faceId];
				var generatedHair = hair[hairId];
				var generatedOutfit = outfits[outfitId];
				var generatedImage = new GeneratedImage
				{
					GeneratedImageId = counter,
					BaseformIdentifier = generatedBaseform.Identifier,
					FaceIdentifier = generatedFace.Identifier,
					HairIdentifier = generatedHair.Identifier,
					OutfitIdentifier = generatedOutfit.Identifier,
					BackgroundIdentifier = generatedBackground.Identifier,
				};

				if (generated.Any(x => x.compare(generatedImage)))
					continue;				

				Console.WriteLine($"Generated image ({dir}/generated/{counter}.png) with traits : baseform-{generatedBaseform.Identifier} | face-{generatedFace.Identifier} | hair-{generatedHair.Identifier} | outfit-{generatedOutfit.Identifier}");				
				generated.Add(generatedImage);
				counter++;
			}
			Console.WriteLine($"Generation completed!!! generated {generated.Count} images");
			return generated;
		}
		public async Task<List<GeneratedImage>> Provide(string dir)
		{
			Console.WriteLine($"Generating images from dir: {dir}");
			var backgrounds = await GetImages(TraitType.Background, dir, "backgrounds").ConfigureAwait(false);
			var baseForms = await GetImages(TraitType.Baseform, dir, "baseforms").ConfigureAwait(false);
			var hair = await GetImages(TraitType.Hair, dir, "hair").ConfigureAwait(false);
			var faces = await GetImages(TraitType.Face, dir, "faces").ConfigureAwait(false);
			var outfits = await GetImages(TraitType.Outfit, dir, "outfits").ConfigureAwait(false);

			var maxnr = outfits.Count * hair.Count * baseForms.Count * faces.Count;
			var rand = new Random();
			var generated = new List<GeneratedImage>();
			int counter = 1;
			Console.WriteLine($"Maximum number of unique combinations: {maxnr}");

			while (counter < maxnr + 1)
			{
				int backgroundId = rand.Next(0, backgrounds.Count);
				int baseformId = rand.Next(0, baseForms.Count);
				int faceId = rand.Next(0, faces.Count);
				int hairId = rand.Next(0, hair.Count);
				int outfitId = rand.Next(0, outfits.Count);

				var generatedBackground = backgrounds[backgroundId];
				var generatedBaseform = baseForms[baseformId];
				var generatedFace = faces[faceId];
				var generatedHair = hair[hairId];
				var generatedOutfit = outfits[outfitId];
				var generatedImage = new GeneratedImage
				{
					GeneratedImageId = counter,
					BaseformIdentifier = generatedBaseform.Identifier,
					FaceIdentifier = generatedFace.Identifier,
					HairIdentifier = generatedHair.Identifier,
					OutfitIdentifier = generatedOutfit.Identifier,
					BackgroundIdentifier = generatedBackground.Identifier,
				};

				if (generated.Any(x => x.compare(generatedImage)))
					continue;

				var background = new Bitmap(generatedBackground.Image);
				var backgroundRect = new Rectangle(Point.Empty, background.Size);

				using (var gr = Graphics.FromImage(background))
				{
					gr.DrawImageUnscaledAndClipped(generatedBaseform.Image, backgroundRect);
					gr.DrawImageUnscaledAndClipped(generatedFace.Image, backgroundRect);
					gr.DrawImageUnscaledAndClipped(generatedOutfit.Image, backgroundRect);
					gr.DrawImageUnscaledAndClipped(generatedHair.Image, backgroundRect);
				}

				Console.WriteLine($"Generated image ({dir}/generated/{counter}.png) with traits : baseform-{generatedBaseform.Identifier} | face-{generatedFace.Identifier} | hair-{generatedHair.Identifier} | outfit-{generatedOutfit.Identifier}");
				generatedImage.generatedImageBitmap = background;
				generated.Add(generatedImage);
				counter++;
			}
			Console.WriteLine($"Generation completed!!! generated {generated.Count} images");
			return generated;
		}

		public async Task<List<TraitImage>> GetImages(TraitType type, string dir, string folder)
		{
			var imageResult = LoadImages(dir, type, folder);
			if (imageResult != null && imageResult.Any())
				return imageResult.ToList();
			else if (type == TraitType.Background)
			{
				Console.WriteLine($"No backgrounds found(at least 1 background needed), stopping..........");
				throw new Exception($"No backgrounds found(at least 1 background needed), stopping..........");
			}
			return new List<TraitImage>();
		}

		private List<TraitImage> LoadImages(string dir, TraitType type, string folder)
		{
			try
			{
				var result = new List<TraitImage>();
				Console.WriteLine($"Loading {type.ToString().ToLower()} images from dir {dir}/{folder}/");
				var files = Directory.GetFiles($"{dir}/{folder}/", "*.png");
				Console.WriteLine($"Found {files.Length} {type.ToString().ToLower()} files!");
				foreach (var file in files)
				{
					Console.WriteLine($"Loading {type.ToString().ToLower()} image {file}");
					result.Add(new TraitImage
					{
						Identifier = Path.GetFileName(file).Replace(".png", ""),
						Type = type,
						Image = Image.FromFile(file)
					});
				}
				return result;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw e;
			}
		}
	}

	public class TraitImage
	{
		public string Identifier { get; set; }
		public Image Image { get; set; }
		public TraitType Type { get; set; }
	}

	public class GeneratedImage
	{
		public int GeneratedImageId { get; set; }
		public string FaceIdentifier { get; set; }
		public string HairIdentifier { get; set; }
		public string BaseformIdentifier { get; set; }
		public string OutfitIdentifier { get; set; }
		public string BackgroundIdentifier { get; set; }

		public Bitmap generatedImageBitmap { get; set; }

		public bool compare(GeneratedImage otherImage)
		{
			return this.FaceIdentifier == otherImage.FaceIdentifier
				&& this.HairIdentifier == otherImage.HairIdentifier
				&& this.BaseformIdentifier == otherImage.BaseformIdentifier
				&& this.OutfitIdentifier == otherImage.OutfitIdentifier;
		}
	}

	public enum TraitType
	{
		Face,
		Hair,
		Outfit,
		Baseform,
		Background,
	}
}
