using EE.BL.Interfaces.Repositories;
using EE.BL.Models;
using System.Threading.Tasks;

namespace EE.BL.Services
{
	public class NftEntityService : INftEntityService
	{
		private readonly INftEntityRepository _repository;
		private readonly IStatsRepository _statsRepository;
		public NftEntityService(INftEntityRepository repository, IStatsRepository statsRepository)
		{
			_repository = repository;
			_statsRepository = statsRepository;
		}		

		public async Task<NftEntity> Get(int id)
		{			
			return await _repository.Get(id).ConfigureAwait(false);				
		}

		public async Task<bool> IsRevealed(int id)
		{
			var totalSupply = await _statsRepository.GetTotalSupply().ConfigureAwait(false);
			return id <= totalSupply;
		}
	}
}
