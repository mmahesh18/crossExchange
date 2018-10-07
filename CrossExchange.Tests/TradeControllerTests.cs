using CrossExchange.Controller;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrossExchange.Tests
{
	class TradeControllerTests
	{
		Mock<IShareRepository> _shareRepositoryMock = new Mock<IShareRepository>();
		Mock<ITradeRepository> _tradeRepositoryMock = new Mock<ITradeRepository>();
		Mock<IPortfolioRepository> _portfolioRepositoryMock = new Mock<IPortfolioRepository>();

		int portfolioId = 1;

		public TradeControllerTests()
		{
			_portfolioRepositoryMock
				.Setup(x => x.GetAsync(It.Is<int>(id => id == portfolioId)))
				.Returns<int>(x => Task.FromResult(new Portfolio() { Id = portfolioId, Name = "John Doe" }));

			_shareRepositoryMock.Setup(x => x.GetBySymbol(It.Is<string>(s => s.Equals("REL"))))
				.Returns(Task.FromResult(new List<HourlyShareRate>(new[]
					{
						new HourlyShareRate() { Symbol = "REL", Rate = 50, TimeStamp = DateTime.Now.AddDays(-1) },
						new HourlyShareRate() { Symbol = "REL", Rate = 100, TimeStamp = DateTime.Now },
						new HourlyShareRate() { Symbol = "REL", Rate = 150, TimeStamp = DateTime.Now.AddDays(-2) },
					}))
				);

			_tradeRepositoryMock.Setup(x => x.GetAllTradings(portfolioId))
				.Returns(Task.FromResult(new List<Trade>(new[]
					{
						new Trade() { Action = "BUY", NoOfShares = 120, PortfolioId = portfolioId, Price = 12000, Symbol = "REL" },
						new Trade() { Action = "SELL", NoOfShares = 40, PortfolioId = portfolioId, Price = 4000, Symbol = "REL" }
					}))
				);
		}

		[Test]
		public async Task Post_ReturnsInvalidPortfolio()
		{
			var tradeController = new TradeController(_shareRepositoryMock.Object, _tradeRepositoryMock.Object, _portfolioRepositoryMock.Object);

			var trade = new TradeModel()
			{
				PortfolioId = -1
			};

			var result = await tradeController.Post(trade) as BadRequestObjectResult;

			Assert.NotNull(result);
			Assert.AreEqual(400, result.StatusCode);
		}

		[Test]
		public async Task Post_ReturnsInvalidShare()
		{
			var tradeController = new TradeController(_shareRepositoryMock.Object, _tradeRepositoryMock.Object, _portfolioRepositoryMock.Object);

			var trade = new TradeModel()
			{
				Symbol = "***"
			};

			var result = await tradeController.Post(trade) as BadRequestObjectResult;

			Assert.NotNull(result);
			Assert.AreEqual(400, result.StatusCode);
		}

		[Test]
		public async Task Post_ReturnsInsufficientNumberOfShares()
		{			
			var tradeController = new TradeController(_shareRepositoryMock.Object, _tradeRepositoryMock.Object, _portfolioRepositoryMock.Object);

			var trade = new TradeModel()
			{
				PortfolioId = 1,
				Action = "SELL",
				Symbol = "REL",
				NoOfShares = 100
			};

			var result = await tradeController.Post(trade) as BadRequestObjectResult;

			Assert.NotNull(result);
			Assert.AreEqual(400, result.StatusCode);
		}

		[Test]
		public async Task Post_ShouldInsertBuyTrade()
		{
			var trade = new TradeModel()
			{
				PortfolioId = 1,
				Action = "BUY",
				Symbol = "REL",
				NoOfShares = 200
			};

			// Act
			var tradeController = new TradeController(_shareRepositoryMock.Object, _tradeRepositoryMock.Object, _portfolioRepositoryMock.Object);

			var result = await tradeController.Post(trade) as CreatedResult;

			// Assert
			Assert.NotNull(result);

			Assert.NotNull(result);
			Assert.AreEqual(201, result.StatusCode);
		}

		[Test]
		public async Task Post_ShouldInsertSellTrade()
		{
			var trade = new TradeModel()
			{
				PortfolioId = 1,
				Action = "SELL",
				Symbol = "REL",
				NoOfShares = 10
			};

			// Act
			var tradeController = new TradeController(_shareRepositoryMock.Object, _tradeRepositoryMock.Object, _portfolioRepositoryMock.Object);

			var result = await tradeController.Post(trade) as CreatedResult;

			// Assert
			Assert.NotNull(result);

			Assert.NotNull(result);
			Assert.AreEqual(201, result.StatusCode);
		}

		[Test]
		public async Task Get_ShouldReturnAllTradings()
		{
			var tradeController = new TradeController(_shareRepositoryMock.Object, _tradeRepositoryMock.Object, _portfolioRepositoryMock.Object);

			var result = await tradeController.GetAllTradings(portfolioId) as OkObjectResult;

			Assert.NotNull(result);

			var resultList = result.Value as List<Trade>;

			Assert.NotNull(result);

			Assert.NotZero(resultList.Count);
		}
	}
}
