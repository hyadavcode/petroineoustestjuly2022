using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Petroineous.Core
{
    public interface IPositionDataService
    {
        Task<IDictionary<DateTime, double>> GetIntradayPowerPositions(DateTime positionDate); 
    }    
}
