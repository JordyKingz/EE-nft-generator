using EE.BL.Models;
using System.Collections.Generic;

namespace ImageGenerationWithWeighting.Models
{
	public class WeightedConfig
	{
		public List<TraitWeights> Traits {  get; set; }
	}

	public class TraitWeights
	{
		public TraitType Type { get; set; }
		public List<TraitPropertyWeight> Properties { get; set; }
	}

	public class TraitPropertyWeight
	{
		public string Name {  get; set; }
		public int Weight { get; set; }
	}
}
