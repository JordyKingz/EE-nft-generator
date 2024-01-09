using EE.BL.Models;
using System.Threading.Tasks;

namespace EE.BL.Interfaces.Repositories
{
	public interface INftEntityRepository
	{
		Task<NftEntity> Get(int id);
		Task Add(NftEntity entity);
	}
}
