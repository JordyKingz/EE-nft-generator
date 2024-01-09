using EE.BL.Models;
using ImageGenerationFinal.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGenerationFinal.Workflow.Providers
{
	public class ImageGenerationProvider
	{
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

		private async Task<List<TraitImage>> GetImages(TraitType type, string dir, string folder)
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
