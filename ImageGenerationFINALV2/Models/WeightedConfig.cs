using EE.BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageGenerationFINALV2.Models
{
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
