using EE.BL.Models;
using System.Threading.Tasks;

namespace EE.BL.Services
{
	public interface INftEntityService
	{
		Task<NftEntity> Get(int id);
		Task<bool> IsRevealed(int id);
	}
}
