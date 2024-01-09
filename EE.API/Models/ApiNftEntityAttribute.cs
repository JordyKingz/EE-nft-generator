using System.Text.Json.Serialization;

namespace EE.API.Models
{
	public class ApiNftEntityAttribute
	{
		[JsonPropertyName("trait_type")]		
		public string TraitType { get; set; }

		[JsonPropertyName("value")]		
		public string Value { get; set; }
	}
}
