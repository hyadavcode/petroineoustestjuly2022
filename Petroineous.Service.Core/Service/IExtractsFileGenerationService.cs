using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Petroineous.Service.Core
{
    public interface IExtractsFileGenerationService
    {
        Task CreatePowerExtractFile(DateTime extractDate, IEnumerable<KeyValuePair<string, string>> data);
    }
}
