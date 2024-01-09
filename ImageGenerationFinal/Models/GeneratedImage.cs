using System.Drawing;

namespace ImageGenerationFinal.Models
{
	public class GeneratedImage
	{
		public int GeneratedImageId { get; set; }
		public string FaceIdentifier { get; set; }
		public string HairIdentifier { get; set; }
		public string BaseformIdentifier { get; set; }
		public string OutfitIdentifier { get; set; }
		public string BackgroundIdentifier { get; set; }

		public Bitmap generatedImageBitmap { get; set;  }

		public bool compare(GeneratedImage otherImage)
		{
			return this.FaceIdentifier == otherImage.FaceIdentifier
				&& this.HairIdentifier == otherImage.HairIdentifier
				&& this.BaseformIdentifier == otherImage.BaseformIdentifier
				&& this.OutfitIdentifier == otherImage.OutfitIdentifier;
		}
	}
}
