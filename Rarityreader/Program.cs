using EE.BL.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Rarityreader
{
	class Program
	{
		static void Main(string[] args)
		{
			var dir = @"C:/EEFINAL";
			var configPath = $"{dir}/generated/configs/entities.json";
			string jsonString = File.ReadAllText(configPath);
			var itemList = JsonSerializer.Deserialize<List<NftEntity>>(jsonString);

			var weightConfig = GetWeightConfig(dir);

			var traits = new List<TraitStats>();
			foreach(var item in weightConfig.Traits)
			{
				var values = new List<TraitStatValue>();
				foreach(var value in item.Properties)
				{
					values.Add(new TraitStatValue
					{
						Identifier = value.Name,
					});
				};
				traits.Add(new TraitStats
				{
					Type = item.Type,
					Values = values,
				});
			}

							

			foreach (var traitGrouped in itemList.SelectMany(x => x.Attributes).GroupBy(z => z.TraitType))
			{
				var list = traitGrouped.ToList();
				var valuesGrouped = list.GroupBy(x => x.Value).ToList();				
				var trait = traits.FirstOrDefault(x => x.Type == traitGrouped.Key);
				foreach (var value in valuesGrouped)
				{
					var temp = Math.Round(Convert.ToDecimal(100 * value.Count()) / Convert.ToDecimal(itemList.Count()), 6);
					var resultValue = trait.Values.FirstOrDefault(x => x.Identifier == value.Key);
					resultValue.Count = value.Count();
					resultValue.Occurence = temp;
				}				
			}

			foreach(var trait in traits)
			{
				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine($"Stats for trait: {trait.Type} with {trait.Values.Sum(z => z.Count)} total and {trait.Values.Sum(x => x.Occurence)}%");

				foreach(var value in trait.Values)
				{
					Console.WriteLine($"           '{value.Identifier}' occured {value.Count} times - {value.Occurence}% have this trait");
				}
			}

			var tokenIdsOfRelic = itemList.Where(x => x.Attributes.Any(z => z.TraitType == TraitType.Relic)).Select(z => z.TokenId).ToList();

			//foreach (var traitGrouped in itemList.SelectMany(x => x.Attributes).GroupBy(z => z.TraitType))
			//{

			//	var list = traitGrouped.ToList();
			//	var valuesGrouped = list.GroupBy(x => x.Value).ToList();
			//	Console.WriteLine($"____----Creating stats for trait: '{traitGrouped.Key}' with {valuesGrouped.Count()} nr of values----____");
			//	foreach (var value in valuesGrouped)
			//	{
			//		var temp = Math.Round(Convert.ToDecimal(100 * value.Count()) / Convert.ToDecimal(itemList.Count()), 6);
			//		Console.WriteLine($"           '{value.Key}' occured {value.Count()} times - {temp}% have this trait");
			//	}
			//	Console.WriteLine();
			//	Console.WriteLine();
			//}
		}

		public class TraitStats
		{
			public TraitType Type { get; set; }
			public List<TraitStatValue> Values { get; set; }
		}

		public class TraitStatValue {
			public string Identifier { get; set; }
			public int Count { get; set; }
			public decimal Occurence { get; set; }
		}

		private static WeightedConfig GetWeightConfig(string dir)
		{
			var path = $"{dir}/weightedconfig.json";

			string jsonString = File.ReadAllText(path);
			var config = JsonSerializer.Deserialize<WeightedConfig>(jsonString);
			return config;
		}

		public class NftEntity
		{
			public int TokenId { get; set; }
			public string Image { get; set; }
			public List<NftEntityAttribute> Attributes { get; set; }
		}

		public class NftEntityAttribute
		{
			public TraitType TraitType { get; set; }
			public string Value { get; set; }
		}

		public class WeightedConfig
		{
			public List<TraitWeights> Traits { get; set; }
		}

		public class TraitWeights
		{
			public TraitType Type { get; set; }
			public List<TraitPropertyWeight> Properties { get; set; }
		}

		public class TraitPropertyWeight
		{
			public string Name { get; set; }
			public int Weight { get; set; }
			public int? Max { get; set; }
		}
	}
}
