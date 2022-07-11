using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Petroineous.Core.Services
{
    public interface IExtractFileGenerationService
    {
        Task CreatePowerExtractFile(DateTime extractDate, IDictionary<DateTime, double> data);
    }
}
