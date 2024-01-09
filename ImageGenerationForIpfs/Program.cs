using EE.BL.Models;
using ImageGenerationFinal.Models;
using ImageGenerationFinal.Workflow.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ImageGenerationForIpfs
{
	class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("Select process..... 1 for image + config generation ==== 2 for metadata generation");
			var choice = Console.ReadLine();
			var dir = @"C:/imageresources";
			var imageUri = "ipfs://QmYgniCwkASmkg3xF2Mob86koxB4g8osNk24TvcMBwgo4u";
			if (choice == "1")
			{
				var provider = new ImageGenerationProvider();
				
				var result = await provider.Provide(dir).ConfigureAwait(false);
				foreach (var item in result)
				{
					Console.WriteLine($"Writing image {item.GeneratedImageId} to local dir: {dir}/generated/");
					Directory.CreateDirectory($"{dir}/generated/images/");
					item.generatedImageBitmap.Save($"{dir}/generated/images/{item.GeneratedImageId}.png");
				}
				Console.WriteLine("Done writing images!!!");
				var mappedList = new List<NftEntity>();
				foreach (var image in result)
					mappedList.Add(Map(image));
				string jsonString = JsonSerializer.Serialize(mappedList);
				Directory.CreateDirectory($"{dir}/generated/configs/");
				File.WriteAllText($"{dir}/generated/configs/imageconfig.json", jsonString);
				Console.WriteLine("Writing imageconfig to file");

			}
			if (choice == "2")
			{
				Console.WriteLine("Generating metadata files for specified config");
				var path = $"{dir}/generated/configs/imageconfig.json";
				string jsonString = File.ReadAllText(path);
				var itemList = JsonSerializer.Deserialize<List<NftEntity>>(jsonString);
				Directory.CreateDirectory($"{dir}/generated/metadata/");
				foreach (var entity in itemList)
				{
					var mapped = new MetaData
					{
						Name = $"Eternal entity #{entity.TokenId}",
						Description = "Eternal entities description here",
						Image = $"{imageUri}/{entity.TokenId}.png",
						Attributes = entity.Attributes.Select(x => new MetaDataAttribute {
							TraitType = x.TraitType.ToString(),
							Value = x.Value
						}).ToList()
					};
					string entityJsonString = JsonSerializer.Serialize(mapped);
					File.WriteAllText($"{dir}/generated/metadata/{entity.TokenId}", entityJsonString);
				}
				Console.WriteLine("Finished generating metadata files for specified config");
			}
			Console.WriteLine("pres any key");
			Console.ReadKey();
		}

		public class MetaData
		{
			[JsonPropertyName("name")]
			public string Name { get; set; }

			[JsonPropertyName("description")]
			public string Description { get; set; }

			[JsonPropertyName("image")]
			public string Image { get; set; }

			[JsonPropertyName("attributes")]
			public List<MetaDataAttribute> Attributes { get; set; }
		}

		public class MetaDataAttribute{
			[JsonPropertyName("trait_type")]
			public string TraitType { get; set; }

			[JsonPropertyName("value")]
			public string Value { get; set; }
		}

		private static NftEntity Map(GeneratedImage generatedImage)
		{
			return new NftEntity
			{
				TokenId = generatedImage.GeneratedImageId,
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
		}
	}
}
