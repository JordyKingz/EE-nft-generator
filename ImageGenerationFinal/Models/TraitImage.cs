using EE.BL.Models;
using System.Drawing;

namespace ImageGenerationFinal.Models
{
	public class TraitImage
	{
		public string Identifier { get; set; }
		public Image Image { get; set; }
		public TraitType Type { get; set; }
	}
}