using EE.API.Models;
using System.Threading.Tasks;

namespace EE.API.Workflow.Providers.Interfaces
{
	public interface IApiNftEntityProvider
	{
		Task<IApiNftEntity> Provide(int id);
	}
}
