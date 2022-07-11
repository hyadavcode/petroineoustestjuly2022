using Petroineous.Service.Core;
using Petroineous.Service.Core.Common;
using System;
using System.ServiceProcess;

namespace Petroineous.Service.Windows
{
    static class Program
    {
        static void Main()
        {
            try
            {                
                Console.WriteLine("Registering dependencies");
                Bootstrapper.RegisterDependencies();

                var extractsOrchestrationSvc = Bootstrapper.Get<IExtractsOrchestrationService>();
                if (extractsOrchestrationSvc == null)
                    throw new OperationCanceledException($"Failed to resolve the dependency {nameof(IExtractsOrchestrationService)}");

                var logger = Bootstrapper.Get<ILogger>();
                if (logger == null)
                    throw new OperationCanceledException($"Failed to resolve the dependency {nameof(ILogger)}");

                // Get service instance
                var svc = new PowerPositionWindowsService(extractsOrchestrationSvc, logger);


                // Run the service
                if (Environment.UserInteractive)
                {
                    Console.WriteLine("Starting service in interactive / debug mode");
                    svc.RunInteractiveMode();
                }
                else
                {
                    Console.WriteLine("Starting service in normal Windows service mode");
                    ServiceBase.Run(new ServiceBase[] { svc });
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error in starting service. {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}
