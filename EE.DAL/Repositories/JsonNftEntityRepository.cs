using EE.BL.Contstants;
using EE.BL.Interfaces.Repositories;
using EE.BL.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace EE.DAL.Repositories
{
	public class JsonNftEntityRepository : INftEntityRepository
	{
		private readonly IMemoryCache _memoryCache;
		public JsonNftEntityRepository(IMemoryCache memoryCache)
		{
			_memoryCache = memoryCache;
		}

		public Task Add(NftEntity entity)
		{
			throw new NotImplementedException();
		}

		public async Task<NftEntity> Get(int id)
		{
			var entities = await GetEntities().ConfigureAwait(false);
			return entities?.FirstOrDefault(x => x.TokenId == id);
		}

		private async Task<List<NftEntity>> GetEntities()
		{
			if (!_memoryCache.TryGetValue($"{CacheKeys.NftEntities}", out List<NftEntity> nftEntitiesCacheEntry))
			{
				string path = Path.Combine($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Resources", @"entities.json");
				string jsonString = File.ReadAllText(path);
				nftEntitiesCacheEntry =  JsonSerializer.Deserialize<List<NftEntity>>(jsonString);
				if (nftEntitiesCacheEntry == null)
					return null;

				var cacheEntryOptions = new MemoryCacheEntryOptions()
						.SetAbsoluteExpiration(TimeSpan.FromHours(24));

				// Save data in cache.
				_memoryCache.Set($"{CacheKeys.NftEntities}", nftEntitiesCacheEntry, cacheEntryOptions);
			}
			return nftEntitiesCacheEntry;
		}
	}
}
