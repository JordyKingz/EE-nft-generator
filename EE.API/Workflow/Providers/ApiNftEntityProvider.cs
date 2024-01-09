using EE.API.Models;
using EE.API.Workflow.Providers.Interfaces;
using EE.BL.Services;
using System.Linq;
using System.Threading.Tasks;

namespace EE.API.Workflow.Providers
{
	public class ApiNftEntityProvider : IApiNftEntityProvider
	{
		private readonly INftEntityService _nftEntityService;
		public ApiNftEntityProvider(INftEntityService nftEntityService)
		{
			_nftEntityService = nftEntityService;
		}

		public async Task<IApiNftEntity> Provide(int id)
		{
			if (id > 10000)
				return null;
			var isRevealed = await _nftEntityService.IsRevealed(id).ConfigureAwait(false);
			
			if (!isRevealed)
			{
				return new ApiNftEntityNoAttributes
				{
					Description = "EternalEntities",
					Name = $"EternalEntity #{id}",
					Image = $"ipfs://QmenX7phVHH8nLoYmpkxfhC1ajmGafe2LYfHqasTgipYdW",//pre reveal GIF
				};
			}
			else
			{
				var nftEntity = await _nftEntityService.Get(id).ConfigureAwait(false);
				return new ApiNftEntity
				{
					Description = "EternalEntities",
					Name = $"EternalEntity #{id}",
					Image = $"https://imaging.eternalentities.io/images/{nftEntity.Image}.png",
					Attributes = nftEntity.Attributes.Select(x => new ApiNftEntityAttribute
					{
						TraitType = x.TraitType.ToString().ToLower(),
						Value = x.Value,
					}).ToList(),
				};
			}			
		}
	}
}
