using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EE.BL.Models
{
	public class NftEntityAttribute
	{
		//[Key]
		//public int Id { get; set; }
		public TraitType TraitType { get; set; }
		public string Value { get; set; }

		//[ForeignKey("NftEntity")]
		//public int NftEntityId { get; set; }
		//public NftEntity NftEntity { get; set; }
	}
}
