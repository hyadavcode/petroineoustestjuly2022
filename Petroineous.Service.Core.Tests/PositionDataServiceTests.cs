using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Petroineous.Service.Core.Tests
{
    [TestClass]
    public class PositionDataServiceTests
    {
        [TestMethod]
        public async Task Test_GetIntradayPowerPositions_Returns_CorrectPositonAggregates()
        {
            // Setup test data
            var testDate = DateTime.Today;
            var testData = new
            {
                Date = testDate,
                Data = new List<Services.PowerTrade>()
                {
                    Services.PowerTrade.Create(testDate, 24),
                    Services.PowerTrade.Create(testDate, 24),
                    Services.PowerTrade.Create(testDate.AddDays(-1), 24)
                }
            };

            testData.Data.ForEach(x =>
            {
                foreach (var p in x.Periods)
                    p.Volume = 10;
            });

            // Setup mocks
            var powerService = new Mock<Services.IPowerService>();
            var logger = new Mock<Petroineous.Service.Core.Common.ILogger>();

            powerService.Setup(x => x.GetTradesAsync(testData.Date))
                .Returns(() => Task.FromResult(testData.Data.AsEnumerable()));

            logger.Setup(x => x.LogInfo(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            logger.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>()))
                .Returns(Task.CompletedTask);


            // create instance 
            var positionSvc = new PositionDataService(powerService.Object, logger.Object);
            var res = await positionSvc.GetIntradayPowerPositions(testDate);

            Assert.AreEqual(24, res.Count);
            Assert.AreEqual(testDate.AddDays(-1).AddHours(23), res.First().Key);
            Assert.AreEqual(23, res.Count(x => x.Key.Date == testDate.Date));
            Assert.IsTrue(res.All(x => x.Value == 20));

            logger.Verify(x => x.LogInfo(It.IsAny<string>()), Times.AtLeastOnce);
            logger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [TestMethod]
        public async Task Test_GetIntradayPowerPositions_Returns_ZeroVolume_WhenPowerServiceReturns_NoData()
        {
            // Setup test data
            var testDate = DateTime.Today;
            var testData = new
            {
                Date = testDate,
                Data = new List<Services.PowerTrade>()
                {
                  // no data
                }
            };

            // Setup mocks
            var powerService = new Mock<Services.IPowerService>();
            var logger = new Mock<Petroineous.Service.Core.Common.ILogger>();

            powerService.Setup(x => x.GetTradesAsync(testData.Date))
                .Returns(() => Task.FromResult(testData.Data.AsEnumerable()));

            logger.Setup(x => x.LogInfo(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            logger.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>()))
                .Returns(Task.CompletedTask);


            // create instance 
            var positionSvc = new PositionDataService(powerService.Object, logger.Object);
            var res = await positionSvc.GetIntradayPowerPositions(testDate);

            Assert.AreEqual(24, res.Count);
            Assert.AreEqual(testDate.AddDays(-1).AddHours(23), res.First().Key);
            Assert.AreEqual(23, res.Count(x => x.Key.Date == testDate.Date));
            Assert.IsTrue(res.All(x => x.Value == 0));

            logger.Verify(x => x.LogInfo(It.IsAny<string>()), Times.AtLeastOnce);
            logger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }
    }
}
