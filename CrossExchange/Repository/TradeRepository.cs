using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrossExchange
{
	public class TradeRepository : GenericRepository<Trade>, ITradeRepository
	{
		public TradeRepository(ExchangeContext dbContext)
		{
			_dbContext = dbContext;
		}

		public Task<List<Trade>> GetAllTradings(int portFolioId)
		{
			return Query().Where(x => x.PortfolioId.Equals(portFolioId)).ToListAsync();
		}
	}
}