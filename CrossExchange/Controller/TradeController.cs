using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;


namespace CrossExchange.Controller
{
	[Route("api/Trade")]
    public class TradeController : ControllerBase
    {
        private IShareRepository _shareRepository { get; set; }
        private ITradeRepository _tradeRepository { get; set; }
        private IPortfolioRepository _portfolioRepository { get; set; }

        public TradeController(IShareRepository shareRepository, ITradeRepository tradeRepository, IPortfolioRepository portfolioRepository)
        {
            _shareRepository = shareRepository;
            _tradeRepository = tradeRepository;
            _portfolioRepository = portfolioRepository;
        }
		
        [HttpGet("{portfolioId}")]
        public async Task<IActionResult> GetAllTradings([FromRoute]int portfolioId)
        {
            var trade = await _tradeRepository.GetAllTradings(portfolioId);
            return Ok(trade);
        }
		
        /*************************************************************************************************************************************
        For a given portfolio, with all the registered shares you need to do a trade which could be either a BUY or SELL trade. For a particular trade keep following conditions in mind:
		BUY:
        a) The rate at which the shares will be bought will be the latest price in the database.
		b) The share specified should be a registered one otherwise it should be considered a bad request. 
		c) The Portfolio of the user should also be registered otherwise it should be considered a bad request. 
                
        SELL:
        a) The share should be there in the portfolio of the customer.
		b) The Portfolio of the user should be registered otherwise it should be considered a bad request. 
		c) The rate at which the shares will be sold will be the latest price in the database.
        d) The number of shares should be sufficient so that it can be sold. 
        Hint: You need to group the total shares bought and sold of a particular share and see the difference to figure out if there are sufficient quantities available for SELL. 

        *************************************************************************************************************************************/

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]TradeModel model)
        {
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			Portfolio portfolio = await _portfolioRepository.GetAsync(model.PortfolioId);

			if (portfolio == null)
			{
				return BadRequest("Invalid portfolio.");
			}

			// Check share is exists and search for the last share rate.

			HourlyShareRate share = (await _shareRepository.GetBySymbol(model.Symbol)).FirstOrDefault();

			if (share == null)
			{
				return BadRequest("Invalid share.");
			}
						
			decimal latestRate = (await _shareRepository.GetBySymbol(model.Symbol))
				.OrderByDescending(x => x.TimeStamp)
				.First().Rate;

			Trade trade;

			if (model.Action == "BUY")
			{
				// Create the BUY trade with lastest price.

				trade = new Trade()
				{
					Action = model.Action,
					PortfolioId = model.PortfolioId,
					Symbol = model.Symbol,
					NoOfShares = model.NoOfShares,
					Price = latestRate * model.NoOfShares
				};
			}
			else // SELL
			{
				// Calculate number of available shares to sell. It's number of shares bought minus number of shares sold.

				var trades = await _tradeRepository.GetAllTradings(portfolio.Id);

				int bought = trades
					.Where(x => x.Action.Equals("BUY") && x.Symbol.Equals(model.Symbol))
					.Sum(x => x.NoOfShares);
				
				int sold = trades
					.Where(x => x.Action.Equals("SELL") && x.Symbol.Equals(model.Symbol))
					.Sum(x => x.NoOfShares);

				int left = bought - sold;

				if (left < model.NoOfShares)
				{
					return BadRequest("Insufficient number of shares.");
				}

				trade = new Trade()
				{
					Action = model.Action,
					PortfolioId = model.PortfolioId,
					Symbol = model.Symbol,
					NoOfShares = model.NoOfShares,
					Price = latestRate * model.NoOfShares
				};
			}
						
			await _tradeRepository.InsertAsync(trade);

			return Created("Trade", trade);
        }
    }
}
