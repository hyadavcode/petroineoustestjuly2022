using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Petroineous.Service.Core.Common;
using Petroineous.Service.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Petroineous.Service.Core.Tests
{
    [TestClass]
    public class ExtractsOrchestrationServiceTests
    {
        [TestMethod]
        public async Task Test_ExtractOrchestrationService_Starts_And_Stops_Correctly()
        {
            // Setup test data
            var testIntervalSeconds = 5;
            var testDate = DateTime.Today;
            var testData = new
            {
                Date = testDate,
                Data = new List<Services.PowerTrade>()
                {
                    Services.PowerTrade.Create(testDate, 24),
                }
            };

            testData.Data.ForEach(x =>
            {
                foreach (var p in x.Periods)
                    p.Volume = 10;
            });

            // Setup mocks            
            var posSvc = new Mock<IPositionDataService>();
            var fileSvc = new Mock<IExtractsFileGenerationService>();
            var config = new Mock<IConfiguration>();
            var logger = new Mock<ILogger>();
            var evtAgg = new EventAggregator(logger.Object);


            logger.Setup(x => x.LogInfo(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            logger.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>()))
                .Returns(Task.CompletedTask);

            config.SetupGet<int>(x => x.IntradayExtractScheduleInterval)
                .Returns(testIntervalSeconds);

            posSvc.Setup(x => x.GetIntradayPowerPositions(It.IsAny<DateTime>()))
                .Returns(Task.FromResult((IDictionary<DateTime, double>)new Dictionary<DateTime, double>()));

            fileSvc.Setup(x => x.CreatePowerExtractFile(It.IsAny<DateTime>(), It.IsAny<IDictionary<string, string>>()))
                .Returns(Task.CompletedTask);

            // setup orchestrator svc
            var orchestrator = new ExtractsOrchestrationService(evtAgg, posSvc.Object, fileSvc.Object, config.Object, logger.Object);

            // Start orchestrator
            await orchestrator.Start();
            Assert.AreEqual(ServiceStatus.Started, orchestrator.Status);
            
            await orchestrator.Stop();
            Assert.AreEqual(ServiceStatus.Stopped, orchestrator.Status);
        }
    }
}
