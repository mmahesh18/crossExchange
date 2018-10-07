using CrossExchange.Controller;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CrossExchange.Tests
{
	class PortfolioControllerTests
	{
		[Test]
		public async Task GetPortfolioInfo_ReturnsNotNull()
		{
			int portfolioId = 1;

			var portfolioRepositoryMock = new Mock<IPortfolioRepository>();

			portfolioRepositoryMock
				.Setup(x => x.GetAsync(It.Is<int>(id => id == portfolioId)))
				.Returns(Task.FromResult(new Portfolio() { Id = portfolioId, Name = "John Doe" }));

			var portfolioController = new PortfolioController(portfolioRepositoryMock.Object);

			var result = await portfolioController.GetPortfolioInfo(portfolioId) as OkObjectResult;

			Assert.NotNull(result);
			
			var resultPortfolio = result.Value as Portfolio;

			Assert.NotNull(resultPortfolio);

			Assert.AreEqual(portfolioId, resultPortfolio.Id);
		}

		[Test]
		public async Task Post_ShouldInsertPortfolio()
		{
			var portfolioRepositoryMock = new Mock<IPortfolioRepository>();

			var portfolioController = new PortfolioController(portfolioRepositoryMock.Object);

			var portfolio = new Portfolio()
			{
				Name = "John Smith"
			};

			// Act
			var result = await portfolioController.Post(portfolio);

			// Assert
			Assert.NotNull(result);

			var createdResult = result as CreatedResult;
			Assert.NotNull(createdResult);
			Assert.AreEqual(201, createdResult.StatusCode);
		}
	}
}
