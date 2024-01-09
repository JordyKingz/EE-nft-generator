using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TempMetaGeneration
{
	class Program
	{
		static void Main(string[] args)
		{
			var dir = @"C:/EE PRELEASE FREEMINT ETC/metadata";
			for (int i = 1; i<=10000; i++)
			{
				var mapped = new MetaData
				{
					Name = $"EternalEntity #{i}",
					Description = "EternalEntities",
					Image = $"ipfs://QmenX7phVHH8nLoYmpkxfhC1ajmGafe2LYfHqasTgipYdW",					
				};
				string entityJsonString = JsonSerializer.Serialize(mapped);
				File.WriteAllText($"{dir}/{i}", entityJsonString);
			}
		}
	}

	public class MetaData
	{
		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("description")]
		public string Description { get; set; }

		[JsonPropertyName("image")]
		public string Image { get; set; }
		
	}
}
