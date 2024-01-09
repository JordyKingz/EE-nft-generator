using System.Text.Json.Serialization;

namespace EE.API.Models
{
	public class ApiNftEntityNoAttributes : IApiNftEntity
	{
		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("description")]
		public string Description { get; set; }

		[JsonPropertyName("image")]
		public string Image { get; set; }
	}
}
