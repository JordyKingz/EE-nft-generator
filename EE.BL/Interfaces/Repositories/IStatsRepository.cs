using System.Threading.Tasks;

namespace EE.BL.Interfaces.Repositories
{
	public interface IStatsRepository
	{
		Task<long> GetTotalSupply();
	}
}
