using Petroineous.Service.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Petroineous.Service.Core
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

                var totalTradeVolumes = trades.SelectMany(x => x.Periods.Select(y => new
                                            {
                                                Date = x.Date,
                                                Hour = y.Period,
                                                Volume = y.Volume
                                            }))
                                            .Where(x => x.Date.Date == positionDate)
                                            .GroupBy(x => x.Hour)
                                            .Select(x => new { Hour = x.Key, Volume = x.Sum(y => y.Volume) });                

                var res = totalTradeVolumes.ToDictionary(x => currentPowerDayStartTime.AddHours(x.Hour - 1), x => x.Volume);

                if (!res.Any())
                {
                    await _logger.LogInfo($"Power service returned no data for the date {positionDate:G}");
                    res = Enumerable.Range(0, 24).ToDictionary(x => currentPowerDayStartTime.AddHours(x), x => 0d);
                }
                        
                return res;
            }
            catch(Exception ex)
            {
                await _logger.LogError($"Error in {nameof(PositionDataService.GetIntradayPowerPositions)}", ex);
                throw;
            }
        }
    }
}
