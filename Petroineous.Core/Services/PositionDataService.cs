using Petroineous.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Petroineous.Core
{
    public class PositionDataService : IPositionDataService
    {
        private readonly Services.IPowerService _powerService;
        private readonly ILogger _logger;

        public PositionDataService(Services.IPowerService powerService, ILogger logger)
        {
            _powerService = powerService;
            _logger = logger;
        }

        public async Task<IDictionary<DateTime, double>> GetIntradayPowerPositions(DateTime positionDate)
        {
            try
            {
                await _logger.LogInfo("Getting power trades from external Power Service");
                var trades = await _powerService.GetTradesAsync(positionDate.Date);

                var currentPowerDayStartTime = positionDate.Date.AddDays(-1).AddHours(23);

                await _logger.LogInfo("Aggregating power trade volume calculation");
                var totalTradeVolumes = trades.SelectMany(x => x.Periods)
                                            .GroupBy(x => x.Period)
                                            .Select(x => new { Period = x.Key, Volume = x.Sum(y => y.Volume) });

                return totalTradeVolumes.ToDictionary(x => currentPowerDayStartTime.AddHours(x.Period - 1), x => x.Volume);
            }
            catch(Exception ex)
            {
                await _logger.LogError($"Error in {nameof(PositionDataService.GetIntradayPowerPositions)}", ex);
                throw;
            }
        }
    }
}
