using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EE.BL.Models;
using EE.BL.Services;
using ImageGenerationFinal.Models;
using ImageGenerationFinal.Workflow.Providers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ImageGenerationFinal.Workflow.Processors
{
	public class ImageGenerationUploader : IImageGenerationUploader
	{
		//private readonly ISettingService _settingService;		
		//private readonly INftEntityService _nftEntityService;
		private readonly ImageGenerationProvider _imageGenerationProvider;
		public ImageGenerationUploader(
			//ISettingService settingService, 
			//INftEntityService nftEntityService, 
			ImageGenerationProvider imageGenerationProvider)
		{
			//_settingService = settingService;
			//_nftEntityService = nftEntityService;
			_imageGenerationProvider = imageGenerationProvider;
		}

		public async Task Process()
		{
			var dir = $"{Directory.GetCurrentDirectory()}/resources";
#if DEBUG
			dir = @"C:/imageresources";
#endif
			var result = await _imageGenerationProvider.Provide(dir).ConfigureAwait(false);
			//var setting = await _settingService.Get().ConfigureAwait(false);
			foreach (var generatedImage in result)
			{
				Console.WriteLine($"Processing generated image {generatedImage.GeneratedImageId} for upload");
				Console.WriteLine("Uploading to azure blob");
				await UploadImageToBlob("https://accountantteststorage.blob.core.windows.net/testimages/", generatedImage.generatedImageBitmap, generatedImage.GeneratedImageId).ConfigureAwait(false);
				//Console.WriteLine("Adding to database");
				//await AddToDatabase(generatedImage).ConfigureAwait(false);
				Console.WriteLine("finished processing image");
			}
			WriteToJsonFile(result, dir);
		}

		private async Task UploadImageToBlob(string baseUri, Bitmap image, int tokenId)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				image.Save(memoryStream, ImageFormat.Png);
				memoryStream.Seek(0, SeekOrigin.Begin); // otherwise you'll get zero byte files
																								//CloudBlockBlob blockBlob = jpegContainer.GetBlockBlobReference(filename);
																								//blockBlob.UploadFromStream(memoryStream);
																								// Create the blob client.
				Uri blobUri = new Uri($"{baseUri}{tokenId}.png");
				StorageSharedKeyCredential storageCredentials =
								new StorageSharedKeyCredential("accountantteststorage", "AvT3dISgBBbOcEBvyqPMXSJqPFJf+qLQHl0QQg2fXkxJPOlFEC5+jgUe9hi9h/SpI+fLFCpzsz6ZJBSf1tDpuQ==");
				BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

				// Upload the file
				var blobHttpHeader = new BlobHttpHeaders();
				blobHttpHeader.ContentType = "image/png";
				var result = await blobClient.UploadAsync(memoryStream, blobHttpHeader).ConfigureAwait(false);
			}
		}

		private void WriteToJsonFile(List<GeneratedImage> images, string dir)
		{
			Console.WriteLine("Writing generated images to json file");
			var mappedList = new List<NftEntity>();
			foreach (var image in images)
				mappedList.Add(Map(image));
			string jsonString = JsonSerializer.Serialize(mappedList);
			File.WriteAllText($"{dir}/generated/entities.json", jsonString);
			Console.WriteLine("Writing generated images to json file finished");
		}

		private NftEntity Map(GeneratedImage generatedImage)
		{
			return new NftEntity
			{
				TokenId = generatedImage.GeneratedImageId,				
				Attributes = new List<NftEntityAttribute>
				{
					new NftEntityAttribute
					{
						TraitType = TraitType.Background,
						Value = generatedImage.BackgroundIdentifier,
					},
					new NftEntityAttribute
					{
						TraitType = TraitType.Baseform,
						Value = generatedImage.BaseformIdentifier,
					},
					new NftEntityAttribute
					{
						TraitType = TraitType.Face,
						Value = generatedImage.FaceIdentifier,
					},
					new NftEntityAttribute
					{
						TraitType = TraitType.Outfit,
						Value = generatedImage.OutfitIdentifier,
					},
					new NftEntityAttribute
					{
						TraitType = TraitType.Hair,
						Value = generatedImage.HairIdentifier,
					},
				}
			};
		}
	}
}
