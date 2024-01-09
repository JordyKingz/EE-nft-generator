using EE.BL.Contstants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace EE.API.Controllers
{
	[ApiController]
	[Route("cache")]
	public class CacheController : ControllerBase
	{
		private readonly IMemoryCache _memCache;

		public CacheController(IMemoryCache memCache)
		{
			_memCache = memCache;
		}

		[HttpGet]
		[Route("invalidate")]
		public async Task<ActionResult> Invalidate(string secret)
		{
			if (secret == null || secret != "d6dcd302-64b7-4526-9db5-683e8afe012b")
				return BadRequest();
			_memCache.Remove(CacheKeys.Settings);
			_memCache.Remove(CacheKeys.NftEntities);
			return Ok();
		}
	}
}
