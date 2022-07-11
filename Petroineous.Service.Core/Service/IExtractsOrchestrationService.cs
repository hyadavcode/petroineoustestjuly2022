using Petroineous.Service.Core.Common;
using System.Threading.Tasks;

namespace Petroineous.Service.Core
{
    public interface IExtractsOrchestrationService
    {
        ServiceStatus Status { get; }

        Task Start();
        Task Stop();
    }
}