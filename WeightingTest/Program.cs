using System;
using System.Collections.Generic;
using System.Linq;

namespace WeightingTest
{
	class Program
	{
		private static Random _rnd = new Random();

		public static class RandomTools
		{
			public static T PickRandomItemWeighted<T>(Random rndm, IList<(T Item, int Weight)> items)
			{
				if (items?.Count == 0)
				{
					return default;
				}

				int offset = 0;
				(T Item, int RangeTo)[] rangedItems = items
						.OrderBy(item => item.Weight)
						.Select(entry => (entry.Item, RangeTo: offset += entry.Weight))
						.ToArray();

				int randomNumber = rndm.Next(items.Sum(item => item.Weight)) + 1;
				return rangedItems.First(item => randomNumber <= item.RangeTo).Item;
			}
		}

		public class Trait
		{
			public string Name { get; set; }
			public Trait(string name)
			{
				Name = name;
			}
		}

		static void Main(string[] args)
		{
			var traits = new List<(Trait, int)>();
			traits.Add((new Trait("70"), 70));
			traits.Add((new Trait("60"), 60));
			traits.Add((new Trait("40"), 40));
			traits.Add((new Trait("30"), 30));
			traits.Add((new Trait("25"), 25));
			traits.Add((new Trait("20"), 20));
			traits.Add((new Trait("10"), 10));
			traits.Add((new Trait("8"), 8));

			while (true)
			{
				Dictionary<string, int> result = new Dictionary<string, int>();

				Trait selectedTrait = null;

				for (int i = 0; i < 1000; i++)
				{
					selectedTrait = RandomTools.PickRandomItemWeighted(_rnd, traits);
					if (selectedTrait != null)
					{
						if (result.ContainsKey(selectedTrait.Name))
						{
							result[selectedTrait.Name] = result[selectedTrait.Name] + 1;
						}
						else
						{
							result.Add(selectedTrait.Name, 1);
						}
					}
				}

				Console.WriteLine("70\t\t" + result["70"]);
				Console.WriteLine("60\t\t" + result["60"]);
				Console.WriteLine("40\t\t" + result["40"]);
				Console.WriteLine("30\t\t" + result["30"]);
				Console.WriteLine("25\t\t" + result["25"]);
				Console.WriteLine("20\t\t" + result["20"]);
				Console.WriteLine("10\t\t" + result["10"]);
				Console.WriteLine("8\t\t" + result["8"]);


				result.Clear();
				Console.WriteLine();
				Console.ReadKey();
			}
		}
	}
}