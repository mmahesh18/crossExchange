using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrossExchange
{
    public interface IShareRepository : IGenericRepository<HourlyShareRate>
    {
		Task<List<HourlyShareRate>> GetBySymbol(string symbol);
	}
}