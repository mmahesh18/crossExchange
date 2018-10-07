using System;
using System.Threading.Tasks;
using CrossExchange.Controller;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace CrossExchange.Tests
{
    public class ShareControllerTests
    {
        private readonly Mock<IShareRepository> _shareRepositoryMock = new Mock<IShareRepository>();

        private readonly ShareController _shareController;

        public ShareControllerTests()
        {
			_shareController = new ShareController(_shareRepositoryMock.Object);
        }

		[Test]
		public async Task Get_ReturnsNotNull()
		{
			string symbol = "REL";

			_shareRepositoryMock
				.Setup(x => x.GetBySymbol(It.Is<string>(s => s == symbol)))
				.Returns(Task.FromResult(new List<HourlyShareRate>(new[]
					{
						new HourlyShareRate() { Id = 1, Symbol = symbol, Rate = 100, TimeStamp = DateTime.Now }
					}))
				);

			var result = await _shareController.Get(symbol) as OkObjectResult;
						
			Assert.NotNull(result);

			var resultList = result.Value as List<HourlyShareRate>;

			Assert.NotNull(resultList);

			Assert.NotZero(resultList.Count);
		}

		[Test]
        public async Task Post_ShouldInsertHourlySharePrice()
        {
            var hourRate = new HourlyShareRate
            {
                Symbol = "CBI",
                Rate = 330.0M,
                TimeStamp = new DateTime(2018, 08, 17, 5, 0, 0)
            };

            // Arrange

            // Act
            var result = await _shareController.Post(hourRate);

            // Assert
            Assert.NotNull(result);

            var createdResult = result as CreatedResult;
            Assert.NotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
        }

		[Test]
		public async Task GetLatestPrice_ShouldReturnLastPrice()
		{
			_shareRepositoryMock.Setup(x => x.GetBySymbol(It.Is<string>(s => s.Equals("REL"))))
				.Returns(Task.FromResult(new List<HourlyShareRate>(new[]
					{
						new HourlyShareRate() { Symbol = "REL", Rate = 50, TimeStamp = DateTime.Now.AddDays(-1) },
						new HourlyShareRate() { Symbol = "REL", Rate = 100, TimeStamp = DateTime.Now },
						new HourlyShareRate() { Symbol = "REL", Rate = 150, TimeStamp = DateTime.Now.AddDays(-2) },
					}))
				);

			var result = await _shareController.GetLatestPrice("REL") as OkObjectResult;

			Assert.AreEqual(100, result.Value);
		}
	}
}
