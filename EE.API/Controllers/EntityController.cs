using EE.API.Workflow.Providers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EE.API.Controllers
{
	[ApiController]
	[Route("entity")]
	public class EntityController : ControllerBase
	{
		private readonly IApiNftEntityProvider _entityProvider;

		public EntityController(IApiNftEntityProvider provider)
		{
			_entityProvider = provider;
		}

		[HttpGet]
		[Route("{id}")]
		public async Task<ActionResult> Get(int id)
		{
			var result = await _entityProvider.Provide(id).ConfigureAwait(false);
			if(result == null)
				return NotFound();
			return Ok(result);
		}		
	}
}
