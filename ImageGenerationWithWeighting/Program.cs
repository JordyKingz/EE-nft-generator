using EE.BL.Models;
using ImageGenerationWithWeighting.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ImageGenerationWithWeighting
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var dir = @"C:/weightedtest";
			var path = $"{dir}/weightedconfig.json";
			string jsonString = File.ReadAllText(path);
			var config = JsonSerializer.Deserialize<WeightedConfig>(jsonString);

			
			var provider = new ImageGenerationProvider();
			var backgrounds = await provider.GetImages(TraitType.Background, dir, "backgrounds").ConfigureAwait(false);
			var baseForms = await provider.GetImages(TraitType.Baseform, dir, "baseforms").ConfigureAwait(false);
			var hair = await provider.GetImages(TraitType.Hair, dir, "hair").ConfigureAwait(false);
			var faces = await provider.GetImages(TraitType.Face, dir, "faces").ConfigureAwait(false);
			var outfits = await provider.GetImages(TraitType.Outfit, dir, "outfits").ConfigureAwait(false);

			var stats = new Dictionary<string, int>();
			foreach(var item in backgrounds)
			{
				stats.Add($"background-{item.Identifier}", 0);
			}
			foreach (var item in baseForms)
			{
				stats.Add($"baseform-{item.Identifier}", 0);
			}
			foreach (var item in hair)
			{
				stats.Add($"hair-{item.Identifier}", 0);
			}
			foreach (var item in faces)
			{
				stats.Add($"face-{item.Identifier}", 0);
			}
			foreach (var item in outfits)
			{
				stats.Add($"outfit-{item.Identifier}", 0);
			}
			var result = await provider.ProvideBase(dir, backgrounds, baseForms, hair, faces, outfits, config, stats).ConfigureAwait(false);
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
		}
	}

	public static class RandomTools
	{
		public static string PickRandomItemWeighted(Random rndm, IList<TraitPropertyWeight> items)
		{
			if (items?.Count == 0)
			{
				return default;
			}

			int offset = 0;
			(string name, int RangeTo)[] rangedItems = items
					.OrderBy(item => item.Weight)
					.Select(entry => (entry.Name, RangeTo: offset += entry.Weight))
					.ToArray();

			int randomNumber = rndm.Next(items.Sum(item => item.Weight)) + 1;
			var item = rangedItems.First(item => randomNumber <= item.RangeTo).name;
			return item;
		}
	}

	public class ImageGenerationProvider
	{
		public async Task<List<GeneratedImage>> ProvideBase(string dir, List<TraitImage> backgrounds, List<TraitImage> baseForms, List<TraitImage> hair, List<TraitImage> faces, List<TraitImage> outfits, WeightedConfig config, Dictionary<string, int> stats)
		{
			Console.WriteLine($"Generating images from dir: {dir}");

			var maxnr = outfits.Count * hair.Count * baseForms.Count * faces.Count;
			
			var generated = new List<GeneratedImage>();
			int counter = 1;
			Console.WriteLine($"Maximum number of unique combinations: {maxnr}");

			var rnd = new Random();
			
			while (counter < 50 + 1)
			{
				var backgroundId = RandomTools.PickRandomItemWeighted(rnd, config.Traits.First(x => x.Type == TraitType.Background).Properties);
				var baseformId = RandomTools.PickRandomItemWeighted(rnd, config.Traits.First(x => x.Type == TraitType.Baseform).Properties);
				var faceId = RandomTools.PickRandomItemWeighted(rnd, config.Traits.First(x => x.Type == TraitType.Face).Properties);
				var hairId = RandomTools.PickRandomItemWeighted(rnd, config.Traits.First(x => x.Type == TraitType.Hair).Properties);
				var outfitId = RandomTools.PickRandomItemWeighted(rnd, config.Traits.First(x => x.Type == TraitType.Outfit).Properties);				

				var generatedBackground = backgrounds.First(x => x.Identifier == backgroundId);
				var generatedBaseform = baseForms.First(x => x.Identifier == baseformId);
				var generatedFace = faces.First(x => x.Identifier == faceId);
				var generatedHair = hair.First(x => x.Identifier == hairId);
				var generatedOutfit = outfits.First(x => x.Identifier == outfitId);

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

				//temp stats
				var backgroundKey = $"background-{backgroundId}";
				if (stats.ContainsKey(backgroundKey))
					stats[backgroundKey] += 1;				

				var baseformKey = $"baseform-{baseformId}";
				if (stats.ContainsKey(baseformKey))
					stats[baseformKey] += 1;

				var faceKey = $"face-{faceId}";
				if (stats.ContainsKey(faceKey))
					stats[faceKey] += 1;

				var hairKey = $"hair-{hairId}";
				if (stats.ContainsKey(hairKey))
					stats[hairKey] += 1;

				var outfitKey = $"outfit-{outfitId}";
				if (stats.ContainsKey(outfitKey))
					stats[outfitKey] += 1;

				Console.WriteLine($"Generated image ({dir}/generated/{counter}.png) with traits : baseform-{generatedBaseform.Identifier} | face-{generatedFace.Identifier} | hair-{generatedHair.Identifier} | outfit-{generatedOutfit.Identifier}");
				generated.Add(generatedImage);
				counter++;
			}

			foreach(var temp in stats.OrderBy(x => x.Key))
			{
				Console.WriteLine($"{temp.Key} occured {temp.Value} times");
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
}
