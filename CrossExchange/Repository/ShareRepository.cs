using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrossExchange
{
    public class ShareRepository : GenericRepository<HourlyShareRate>, IShareRepository
    {
        public ShareRepository(ExchangeContext dbContext)
        {
            _dbContext = dbContext;
        }

		public Task<List<HourlyShareRate>> GetBySymbol(string symbol)
		{
			return Query().Where(x => x.Symbol.Equals(symbol)).ToListAsync();
		}
	}
}