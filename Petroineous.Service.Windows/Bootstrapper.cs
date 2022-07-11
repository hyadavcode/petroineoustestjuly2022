using Autofac;
using Petroineous.Service.Core;
using Petroineous.Service.Core.Common;
using Petroineous.Service.Core.Configuration;

namespace Petroineous.Service.Windows
{
    public class Bootstrapper
    {
        private static IContainer _container;

        public static void RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<Services.PowerService>().As<Services.IPowerService>().SingleInstance();
            builder.RegisterType<Logger>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<Configuration>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<EventAggregator>().AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<ExtractsFileGenerationService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<PositionDataService>().AsImplementedInterfaces();
            builder.RegisterType<ExtractsOrchestrationService>().AsImplementedInterfaces().SingleInstance();

            _container = builder.Build();
        }

        public static T Get<T>()
        {
            if (_container.TryResolve<T>(out T instance))
                return instance;

            return default(T);
        }
    }
}
