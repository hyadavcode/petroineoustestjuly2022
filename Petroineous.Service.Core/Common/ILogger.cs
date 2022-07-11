using System;
using System.Threading.Tasks;

namespace Petroineous.Service.Core.Common
{
    public interface ILogger
    {
        Task LogError(string error, Exception ex = null);
        Task LogInfo(string info);
    }
}
