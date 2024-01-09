using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EE.BL.Models;
using ImageGenerationFINALV2.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ImageGenerationFINALV2
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var dir = @"C:/EEFINAL";
			var path = $"{dir}/weightedconfig.json";

			string jsonString = File.ReadAllText(path);
			var config = JsonSerializer.Deserialize<WeightedConfig>(jsonString);

			var provider = new ImageGenerationProvider();
			var backgrounds = await provider.GetImages(TraitType.Background, dir, "backgrounds").ConfigureAwait(false);
			var baseForms = await provider.GetImages(TraitType.Baseform, dir, "baseforms").ConfigureAwait(false);
			var hair = await provider.GetImages(TraitType.Hair, dir, "hair").ConfigureAwait(false);
			var faces = await provider.GetImages(TraitType.Face, dir, "faces").ConfigureAwait(false);
			var outfits = await provider.GetImages(TraitType.Outfit, dir, "outfits").ConfigureAwait(false);
			var relics = await provider.GetImages(TraitType.Relic, dir, "relics").ConfigureAwait(false);

			var result = await provider.ProvideCompiled(dir, backgrounds, baseForms, hair, faces, outfits, relics, config).ConfigureAwait(false);

			int i = 1;
			foreach (var item in result)
			{
				Console.WriteLine($"Writing image {item.GeneratedImageId} to local dir: {dir}/generated/");
				//Directory.CreateDirectory($"{dir}/generated/images/");
				var background = new Bitmap(backgrounds.First(x => x.Identifier == item.BackgroundIdentifier).Image);
				var backgroundRect = new Rectangle(Point.Empty, background.Size);

				using (var gr = Graphics.FromImage(background))
				{
					gr.DrawImageUnscaledAndClipped(baseForms.First(x => x.Identifier == item.BaseformIdentifier).Image, backgroundRect);
					gr.DrawImageUnscaledAndClipped(faces.First(x => x.Identifier == item.FaceIdentifier).Image, backgroundRect);
					gr.DrawImageUnscaledAndClipped(outfits.First(x => x.Identifier == item.OutfitIdentifier).Image, backgroundRect);
					gr.DrawImageUnscaledAndClipped(hair.First(x => x.Identifier == item.HairIdentifier).Image, backgroundRect);
					if (item.RelicIdentifier != null)
						gr.DrawImageUnscaledAndClipped(relics.First(x => x.Identifier == item.RelicIdentifier).Image, backgroundRect);
				}
				await UploadImageToBlob("https://eenftimage.blob.core.windows.net/images/", background, item.UniqueId, i).ConfigureAwait(false);
				i++;
			}

			Console.WriteLine("Done writing images!!!");
			var mappedList = new List<NftEntity>();
			foreach (var image in result)
				mappedList.Add(Map(image));
			string configJsonString = JsonSerializer.Serialize(mappedList);
			Directory.CreateDirectory($"{dir}/generated/configs/");
			File.WriteAllText($"{dir}/generated/configs/entities.json", configJsonString);
			Console.WriteLine("Writing imageconfig to file");
		}

		private static async Task UploadImageToBlob(string baseUri, Bitmap image, string imageId, int tokenId)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				image.Save(memoryStream, ImageFormat.Png);
				memoryStream.Seek(0, SeekOrigin.Begin); // otherwise you'll get zero byte files
																								//CloudBlockBlob blockBlob = jpegContainer.GetBlockBlobReference(filename);
																								//blockBlob.UploadFromStream(memoryStream);
																								// Create the blob client.
				Uri blobUri = new Uri($"{baseUri}{imageId}.png");
				StorageSharedKeyCredential storageCredentials =
								new StorageSharedKeyCredential("eenftimage", "v4zbJSvSV6AbCwRT6KSsMnHDDEKauqQ1HgjS7Yf0xZblXF502qlU01tgIaXm2hZY25FM1+OkZP5yqgL6q4jq1Q==");
				BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

				// Upload the file
				var blobHttpHeader = new BlobHttpHeaders();
				blobHttpHeader.ContentType = "image/png";
				var result = await blobClient.UploadAsync(memoryStream, blobHttpHeader).ConfigureAwait(false);
				Console.WriteLine($"Uploaded token{tokenId} image to azure!");
			}
		}

		public class ImageGenerationProvider
		{
			private bool IsMax(WeightedConfig config, TraitType type, string id, Dictionary<string, int> maxDictionary)
			{
				if (config.Traits.FirstOrDefault(x => x.Type == type).Properties.FirstOrDefault(x => x.Name == id).Max.HasValue)
				{
					var traitConfigMax = config.Traits.FirstOrDefault(x => x.Type == type).Properties.FirstOrDefault(x => x.Name == id).Max;
					var key = $"{type}{id}";
					if (maxDictionary.ContainsKey(key))
					{
						if (maxDictionary[key] >= traitConfigMax)
						{
							var prop = config.Traits.FirstOrDefault(x => x.Type == type).Properties.FirstOrDefault(x => x.Name == id);
							config.Traits.FirstOrDefault(x => x.Type == type).Properties.Remove(prop);
							return true;
						}
						maxDictionary[key] += 1;
					}
					else
					{
						maxDictionary[key] = 1;
					}
				}
				return false;
			}

			public bool IsCharacterCollision(string hairId, string outfitId, string faceId)
			{
				var nameList = new List<string>();
				nameList.Add(hairId.Split()[0].ToLower());
				nameList.Add(outfitId.Split()[0].ToLower());
				nameList.Add(faceId.Split()[0].ToLower());

				var grouped = nameList.GroupBy(x => x);
				if (grouped.Any(x => x.Count() > 1))									
					return true;				
					
				return false;
			}

			private bool CanGetRelic(Random random)
			{
				var rngNumber = random.Next(1, 100);
				if (rngNumber <= 16)
					return true;
				return false;
			}

			public async Task<List<GeneratedImage>> ProvideCompiled(string dir, List<TraitImage> backgrounds, List<TraitImage> baseforms, List<TraitImage> hair, 
				List<TraitImage> faces, List<TraitImage> outfits, List<TraitImage> relics, WeightedConfig config)
			{
				var maxAmount = 10000;
				var generated = new List<GeneratedImage>();
				int counter = 1;
				var maxDictionary = new Dictionary<string, int>();
				int nrOfRelics = 0;
				int maxNrOfRelics = 1500;

				var rnd = new Random();
				while (counter < maxAmount + 1)
				{
					var hairId = RandomTools.PickRandomItemWeighted(rnd, config.Traits.First(x => x.Type == TraitType.Hair).Properties);
					if (IsMax(config, TraitType.Hair, hairId, maxDictionary))
						continue;
					var outfitId = RandomTools.PickRandomItemWeighted(rnd, config.Traits.First(x => x.Type == TraitType.Outfit).Properties);
					if (IsMax(config, TraitType.Outfit, outfitId, maxDictionary))
						continue;
					var faceId = RandomTools.PickRandomItemWeighted(rnd, config.Traits.First(x => x.Type == TraitType.Face).Properties);
					if (IsMax(config, TraitType.Face, faceId, maxDictionary))
						continue;

					if (IsCharacterCollision(hairId, outfitId, faceId))
						continue;

					var backgroundId = RandomTools.PickRandomItemWeighted(rnd, config.Traits.First(x => x.Type == TraitType.Background).Properties);
					if (IsMax(config, TraitType.Background, backgroundId, maxDictionary))
						continue;
					var baseFormId = RandomTools.PickRandomItemWeighted(rnd, config.Traits.First(x => x.Type == TraitType.Baseform).Properties);
					if (IsMax(config, TraitType.Baseform, baseFormId, maxDictionary))
						continue;
								

					try
					{
						var generatedBackground = backgrounds.First(x => x.Identifier == backgroundId);
						var generatedBaseform = baseforms.First(x => x.Identifier == baseFormId);
						var generatedHair = hair.First(x => x.Identifier == hairId);
						var generatedOutfit = outfits.First(x => x.Identifier == outfitId);
						var generatedFace = faces.First(x => x.Identifier == faceId);

						var generatedImage = new GeneratedImage
						{
							GeneratedImageId = counter,
							BackgroundIdentifier = generatedBackground.Identifier,
							BaseformIdentifier = generatedBaseform.Identifier,
							FaceIdentifier = generatedFace.Identifier,
							HairIdentifier = generatedHair.Identifier,
							OutfitIdentifier = generatedOutfit.Identifier,
							UniqueId = Guid.NewGuid().ToString().Replace("-", ""),							
						};

						if (generated.Any(x => x.compare(generatedImage)))
						{
							continue;
						}

						string relicId = null;
						if (nrOfRelics < maxNrOfRelics)
						{
							if (CanGetRelic(rnd))
							{
								relicId = RandomTools.PickRandomItemWeighted(rnd, config.Traits.First(x => x.Type == TraitType.Relic).Properties);
								nrOfRelics++;
							}
						}
						generatedImage.RelicIdentifier = !string.IsNullOrEmpty(relicId) ? relics.First(x => x.Identifier == relicId).Identifier : null;

						Console.WriteLine($"Generated image ({dir}/generated/{counter}.png) with traits : baseform-{generatedBaseform.Identifier} | face-{generatedFace.Identifier} | hair-{generatedHair.Identifier} | outfit-{generatedOutfit.Identifier}");
						generated.Add(generatedImage);
						counter++;
					}
					catch (Exception ex)
					{
						var a = 1;
					}
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

		private static NftEntity Map(GeneratedImage generatedImage)
		{
			var entity = new NftEntity
			{
				TokenId = generatedImage.GeneratedImageId,
				Image = generatedImage.UniqueId,
				Attributes = new List<NftEntityAttribute>
				{
					new NftEntityAttribute
					{
						TraitType = TraitType.Background,
						Value = generatedImage.BackgroundIdentifier,
					},
					new NftEntityAttribute
					{
						TraitType = TraitType.Baseform,
						Value = generatedImage.BaseformIdentifier,
					},
					new NftEntityAttribute
					{
						TraitType = TraitType.Face,
						Value = generatedImage.FaceIdentifier,
					},
					new NftEntityAttribute
					{
						TraitType = TraitType.Outfit,
						Value = generatedImage.OutfitIdentifier,
					},
					new NftEntityAttribute
					{
						TraitType = TraitType.Hair,
						Value = generatedImage.HairIdentifier,
					},
				}
			};
			if (!string.IsNullOrEmpty(generatedImage.RelicIdentifier))
				entity.Attributes.Add(new NftEntityAttribute
				{
					TraitType = TraitType.Relic,
					Value = generatedImage.RelicIdentifier,
				});
			return entity;
		}
	}

	public static class RandomTools
	{
		public static bool CheckMax(TraitPropertyWeight item)
		{
			return item.Max.HasValue;
		}

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

	public class GeneratedImage
	{
		public string UniqueId { get; set; }
		public int GeneratedImageId { get; set; }
		public string FaceIdentifier { get; set; }
		public string HairIdentifier { get; set; }
		public string BaseformIdentifier { get; set; }
		public string OutfitIdentifier { get; set; }
		public string BackgroundIdentifier { get; set; }
		public string RelicIdentifier { get; set; }

		public bool compare(GeneratedImage otherImage)
		{
			return this.FaceIdentifier == otherImage.FaceIdentifier
				&& this.HairIdentifier == otherImage.HairIdentifier
				&& this.BaseformIdentifier == otherImage.BaseformIdentifier
				&& this.OutfitIdentifier == otherImage.OutfitIdentifier;
		}
	}
	public class TraitImage
	{
		public string Identifier { get; set; }
		public Image Image { get; set; }
		public TraitType Type { get; set; }
	}

}
