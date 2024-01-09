using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EE.BL.Models
{
	public class NftEntity
	{
		//[Key]
		//public int Id { get; set; }		
		public int TokenId { get; set; }
		public string Image { get; set; }
		public List<NftEntityAttribute> Attributes { get; set; }
	}
}
