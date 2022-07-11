using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Petroineous.Service.Core
{
    public interface IPositionDataService
    {
        Task<IDictionary<DateTime, double>> GetIntradayPowerPositions(DateTime positionDate); 
    }    
}
